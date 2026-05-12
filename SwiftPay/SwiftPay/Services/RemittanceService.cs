using Microsoft.EntityFrameworkCore;

using System.Threading.Tasks;
using AutoMapper;
using SwiftPay.Constants.Enums;
using SwiftPay.Domain.Remittance.Entities;
using SwiftPay.DTOs.RemittanceDTO;
using SwiftPay.DTOs.UserCustomerDTO;
using SwiftPay.Models;
using SwiftPay.Repositories.Interfaces;
using SwiftPay.Services.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System;


namespace SwiftPay.Services
{
	public class RemittanceService : IRemittanceService
	{
		private readonly IRemittanceRepository _repo;
		private readonly IRemitValidationRepository _validationRepo;
		private readonly IMapper _mapper;
		private readonly IFXQuoteRepository _quoteRepo;
		private readonly ICustomerRepository _customerRepo;
		private readonly INotificationAlertService _notificationService;

		public RemittanceService(
			IRemittanceRepository repo,
			IRemitValidationRepository validationRepo,
			IMapper mapper,
			IFXQuoteRepository quoteRepo,
			ICustomerRepository customerRepo,
			INotificationAlertService notificationService)
		{
			_repo = repo;
			_validationRepo = validationRepo;
			_mapper = mapper;
			_quoteRepo = quoteRepo;
			_customerRepo = customerRepo;
			_notificationService = notificationService;
		}

		public async Task<List<CreateRemittanceResponseDto>> GetByCustomerRemittancesAsync(int customerId, int page, int limit, string? status = null)
		{
			var list = await _repo.GetByCustomerIdAsync(customerId, page, limit, status);
			return _mapper.Map<List<CreateRemittanceResponseDto>>(list);
		}

		public async Task<string> CancelAsync(int remitId, string cancellationReason)
		{
			var remittance = await _repo.GetByIdAsync(remitId);
			if (remittance == null) throw new Exception("Remittance not found.");

			// Reject cancellation if already Processing or Paid
			if (remittance.Status == RemittanceRequestStatus.Paid || remittance.Status == RemittanceRequestStatus.Queued)
				throw new InvalidOperationException("Cannot cancel a remittance that is already processing or paid.");

			remittance.Status = RemittanceRequestStatus.Cancelled;
			remittance.UpdateDate = DateTime.UtcNow;
			await _repo.UpdateAsync(remittance);

			await TryNotifyCustomerAsync(remittance.CustomerId, remittance.RemitId,
				NotificationCategory.Payout,
				$"Your remittance #{remittance.RemitId} has been cancelled. Reason: {cancellationReason}");

			// generate mock refund reference
			var refundRef = $"REFUND-{Guid.NewGuid():N}";
			// Optionally persist refundRef in RefundRefs table - omitted for Phase-1
			return refundRef;
		}

		public async Task MarkPendingComplianceAsync(int remitId)
		{
			var remittance = await _repo.GetByIdAsync(remitId);
			if (remittance == null) throw new Exception("Remittance not found.");

			remittance.Status = RemittanceRequestStatus.PendingCompliance;
			remittance.UpdateDate = DateTime.UtcNow;
			await _repo.UpdateAsync(remittance);
		}

		/// <summary>
		/// Creates a remittance in Draft state.
		/// ReceiverAmount, RateApplied and FeeApplied are derived from the linked FX quote,
		/// not accepted from the client, keeping the DTO aligned with the SRS spec.
		/// </summary>
		public async Task<CreateRemittanceResponseDto> CreateAsync(CreateRemittanceDto dto)
		{
			// Look up the quote to copy rate/fee/receiverAmount onto the remittance.
			// Per SRS, these fields live on the quote entity, not on the create request.
			decimal? receiverAmount = null;
			decimal? rateApplied = null;
			decimal? feeApplied = null;

			if (!string.IsNullOrWhiteSpace(dto.QuoteId))
			{
				var quote = await _quoteRepo.GetQuoteByIdAsync(dto.QuoteId);
				if (quote != null)
				{
					receiverAmount = quote.ReceiverAmount;
					rateApplied = quote.OfferedRate;
					feeApplied = quote.Fee;
				}
			}

			var remittance = new RemittanceRequest
			{
				// IDs - Use only the IDs, let EF handle the relationship
				CustomerId = dto.CustomerId,
				BeneficiaryId = dto.BeneficiaryId,

				// Currencies - Ensure these aren't null!
				FromCurrency = dto.FromCurrency ?? "USD",
				ToCurrency = dto.ToCurrency ?? "INR",

				// Amounts & Codes
				SendAmount = dto.SendAmount,
				PurposeCode = dto.PurposeCode,
				SourceOfFunds = dto.SourceOfFunds,

				// Quote linkage — QuoteId stored for reference; derived fields fetched from quote above
				QuoteId = dto.QuoteId,
				ReceiverAmount = receiverAmount,
				RateApplied = rateApplied,
				FeeApplied = feeApplied,

				// Metadata
				Status = RemittanceRequestStatus.Draft,
				CreatedDate = DateTime.UtcNow,
				UpdateDate = DateTime.UtcNow,
				IsDeleted = false
			};

			try
			{
				await _repo.CreateAsync(remittance);
			}
			catch (Exception ex)
			{
				// This will help you see the REAL error in your logs
				throw new Exception($"DB Save Error: {ex.InnerException?.Message ?? ex.Message}");
			}

			await TryNotifyCustomerAsync(remittance.CustomerId, remittance.RemitId,
				NotificationCategory.Payout,
				$"Your remittance #{remittance.RemitId} ({remittance.FromCurrency} {remittance.SendAmount:0.00} → {remittance.ToCurrency}) has been submitted successfully.");

			return _mapper.Map<CreateRemittanceResponseDto>(remittance);
		}

		/// <summary>
		/// Gets remittance by ID (string RemitId).
		/// </summary>
		public async Task<CreateRemittanceResponseDto?> GetByIdAsync(int remitId)
		{
			var entity = await _repo.GetByIdAsync(remitId);

			if (entity == null)
				return null;

			return _mapper.Map<CreateRemittanceResponseDto>(entity);
		}


		public async Task<ValidateRemittanceResponseDto> ValidateAsync(int remitId)
		{
			if (remitId <= 0)
				throw new ArgumentException("Invalid remittance ID.");

			try
			{
				// Fetch remittance
				var remittance = await _repo.GetByIdAsync(remitId);
				if (remittance == null)
					throw new Exception("Remittance not found.");

				// Log the current status of the remittance
				Console.WriteLine($"Remittance ID: {remitId}, Status: {remittance.Status}");

				// Allow validation to run if it's Draft OR PendingCompliance
				if (remittance.Status != RemittanceRequestStatus.Draft &&
					remittance.Status != RemittanceRequestStatus.PendingCompliance)
				{
					throw new Exception("Only draft or pending compliance remittances can be validated.");
				}

				var validations = new List<RemitValidation>();

				// Corridor rule (example)
				validations.Add(new RemitValidation
				{
					ValidationId = Guid.NewGuid(),
					RemitId = remittance.RemitId,
					RuleName = ValidationRuleName.Corridor,
					Result = ValidationResult.Pass,
					Message = string.Empty,
				});

				// Limit rule (example – real logic later)
				validations.Add(new RemitValidation
				{
					ValidationId = Guid.NewGuid(),
					RemitId = remittance.RemitId,
					RuleName = ValidationRuleName.Limit,
					Result = ValidationResult.Pass,
					Message = string.Empty,
				});

				// Docs rule example (FAIL case)
				bool hasInvoice = remittance.Documents != null && remittance.Documents.Any(d => d.DocType == DocumentType.Invoice);

				if (!hasInvoice)
				{
					validations.Add(new RemitValidation
					{
						ValidationId = Guid.NewGuid(),
						RemitId = remittance.RemitId,
						RuleName = ValidationRuleName.Docs,
						Result = ValidationResult.Fail,
						Message = "Invoice document is required.",
					});
				}
				else
				{
					validations.Add(new RemitValidation
					{
						ValidationId = Guid.NewGuid(),
						RemitId = remittance.RemitId,
						RuleName = ValidationRuleName.Docs,
						Result = ValidationResult.Pass,
						Message = string.Empty,
						CheckedDate = DateTime.UtcNow
					});
				}

				// Save validation records via validation repository
				if (validations.Any())
				{
					await _validationRepo.AddRangeAsync(validations);
				}

				// Determine overall result
				bool hasFailure = validations.Any(v => v.Result == ValidationResult.Fail);

				remittance.Status = hasFailure
					? RemittanceRequestStatus.Draft
					: RemittanceRequestStatus.Validated;

				await _repo.UpdateAsync(remittance);

				// Map validation entities -> DTOs using AutoMapper
				var validationDtos = _mapper.Map<List<RemitValidationDto>>(validations);

				// Build response using AutoMapper-mapped validation DTOs
				return new ValidateRemittanceResponseDto
				{
					RemitId = remittance.RemitId,
					Status = remittance.Status.ToString(),
					Validations = validationDtos
				};
			}
			catch (DbUpdateException ex)
			{
				throw new Exception("Validation failed. Inner exception: " + ex.InnerException?.Message, ex);
			}
		}


		public async Task UpdateAsync(int remitId, UpdateRemittanceDto dto)
		{
			var remittance = await _repo.GetByIdAsync(remitId);
			if (remittance == null || remittance.IsDeleted)
				throw new KeyNotFoundException("Remittance not found or already deleted.");

			remittance.SendAmount = dto.SendAmount;
			remittance.PurposeCode = dto.PurposeCode;
			remittance.SourceOfFunds = dto.SourceOfFunds;
			remittance.UpdateDate = DateTime.UtcNow;

			await _repo.UpdateAsync(remittance);
		}

		public async Task<bool> DeleteAsync(int remitId)
		{
			if (remitId <= 0)
				throw new ArgumentException("Invalid remittance ID.");

			var remittance = await _repo.GetByIdAsync(remitId);
			if (remittance == null || remittance.IsDeleted)
				throw new KeyNotFoundException("Remittance not found or already deleted.");

			// Soft delete
			remittance.IsDeleted = true;
			remittance.UpdateDate = DateTime.UtcNow;
			await _repo.UpdateAsync(remittance);
			return true;
		}

		public async Task SoftDeleteAsync(int remitId)
		{
			var remittance = await _repo.GetByIdAsync(remitId);
			if (remittance == null || remittance.IsDeleted)
				throw new KeyNotFoundException("Remittance not found or already deleted.");

			remittance.IsDeleted = true;
			remittance.UpdateDate = DateTime.UtcNow;
			await _repo.UpdateAsync(remittance);
		}

		public async Task UpdateValidationAsync(RemitValidationDto dto)
		{
			var existing = await _validationRepo.GetByIdAsync(dto.ValidationId);
			if (existing == null)
				throw new Exception("Validation record not found.");

			// Ensure remit id matches
			if (existing.RemitId != dto.RemitId)
				throw new Exception("RemitId mismatch.");

			// update fields
			existing.RuleName = Enum.TryParse<ValidationRuleName>(dto.Rule, out var rn) ? rn : existing.RuleName;
			existing.Result = Enum.TryParse<ValidationResult>(dto.Result, out var res) ? res : existing.Result;
			existing.Message = dto.Message ?? string.Empty;
			existing.CheckedDate = dto.CheckedDate.UtcDateTime; // Fixed type mismatch
			existing.UpdateDate = DateTime.UtcNow;

			await _validationRepo.UpdateAsync(existing);
		}

		public async Task DeleteValidationAsync(Guid validationId)
		{
			await _validationRepo.DeleteAsync(validationId);
		}



		public async Task<List<RemitValidationDto>> GetValidationsAsync(int remitId)
		{
			if (remitId <= 0)
				throw new ArgumentException("Invalid remittance ID.");

			var validations = await _validationRepo.GetByRemitIdAsync(remitId);

			if (validations == null || validations.Count == 0)
				return new List<RemitValidationDto>();

			// Map using AutoMapper
			return _mapper.Map<List<RemitValidationDto>>(validations);
		}

		public async Task<IEnumerable<RemitValidationDto>> GetValidationsByRemitIdAsync(int remitId)
		{
			var validations = await _validationRepo.GetByRemitIdAsync(remitId);
			return validations.Select(v => new RemitValidationDto
			{
				ValidationId = v.ValidationId,
				RemitId = v.RemitId,
				Rule = v.RuleName.ToString(),
				Result = v.Result.ToString(),
				Message = v.Message,
				IsDeleted = v.IsDeleted // Fixed missing property
			});
		}

		public async Task<RemitValidationDto> GetValidationByIdAsync(Guid validationId)
		{
			var validation = await _validationRepo.GetByIdAsync(validationId);
			if (validation == null) return null;

			return new RemitValidationDto
			{
				ValidationId = validation.ValidationId,
				RemitId = validation.RemitId,
				Rule = validation.RuleName.ToString(),
				Result = validation.Result.ToString(),
				Message = validation.Message,
				IsDeleted = validation.IsDeleted // Fixed missing property
			};
		}

		public async Task<IEnumerable<CreateRemittanceResponseDto>> GetAllAsync()
		{
			var remittances = await _repo.GetAllAsync();
			// Use AutoMapper to map ALL fields (FromCurrency, ToCurrency, SendAmount, CreatedDate, etc.)
			return _mapper.Map<List<CreateRemittanceResponseDto>>(remittances);
		}

		private async Task TryNotifyCustomerAsync(int customerId, int remitId, NotificationCategory category, string message)
		{
			try
			{
				var customer = await _customerRepo.GetByIdAsync(customerId);
				if (customer == null) return;

				await _notificationService.CreateAsync(new CreateNotificationDto
				{
					UserID   = customer.UserID,
					RemitID  = remitId,
					Message  = message,
					Category = category,
				});
			}
			catch { /* notification failure must never block the main flow */ }
		}

		public async Task UpdateVerificationStatusByRemitIdAsync(int remitId, RemittanceRequestStatus status)
		{
			var remittance = await _repo.GetByIdAsync(remitId);
			if (remittance == null || remittance.IsDeleted)
				throw new KeyNotFoundException("Remittance not found or already deleted.");

			// Update the remittance request status
			remittance.Status = status;
			remittance.UpdateDate = DateTime.UtcNow;

			await _repo.UpdateAsync(remittance);

			string? message = status switch
			{
				RemittanceRequestStatus.Validated       => $"Your remittance #{remitId} has been validated and is ready for processing.",
				RemittanceRequestStatus.Queued          => $"Your remittance #{remitId} has been queued for payout.",
				RemittanceRequestStatus.Paid            => $"Your remittance #{remitId} has been paid successfully. Funds have been sent to the beneficiary.",
				RemittanceRequestStatus.Cancelled       => $"Your remittance #{remitId} has been cancelled.",
				RemittanceRequestStatus.ComplianceHold  => $"Your remittance #{remitId} has been placed on compliance hold. Our team is reviewing your transaction.",
				RemittanceRequestStatus.PendingCompliance => $"Your remittance #{remitId} has been sent for compliance review.",
				_ => null
			};

			if (message != null)
				await TryNotifyCustomerAsync(remittance.CustomerId, remitId, NotificationCategory.Payout, message);
		}
	}
}