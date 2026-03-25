using System.Threading.Tasks;
using AutoMapper;
using SwiftPay.Constants.Enums;
using SwiftPay.Domain.Remittance.Entities;
using SwiftPay.DTOs.RemittanceDTO;
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

        public RemittanceService(IRemittanceRepository repo, IRemitValidationRepository validationRepo, IMapper mapper)
        {
            _repo = repo;
            _validationRepo = validationRepo;
            _mapper = mapper;
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
			if (remittance.Status == RemittanceRequestStatus.Paid || remittance.Status == RemittanceRequestStatus.Routing || remittance.Status == RemittanceRequestStatus.Queued)
				throw new InvalidOperationException("Cannot cancel a remittance that is already processing or paid.");

			remittance.Status = RemittanceRequestStatus.Cancelled;
			remittance.UpdateDate = DateTime.UtcNow;
			await _repo.UpdateAsync(remittance);

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
		/// </summary>
		public async Task<CreateRemittanceResponseDto> CreateAsync(CreateRemittanceDto dto)
		{
			var remittance = new RemittanceRequest
			{
				SendAmount = dto.SendAmount,
				PurposeCode = dto.PurposeCode,
				SourceOfFunds = dto.SourceOfFunds,
				Status = RemittanceRequestStatus.Draft,
				IsDeleted = false,
				CreatedDate = DateTime.UtcNow,

				CustomerId = dto.CustomerId,
				BeneficiaryId = dto.BeneficiaryId,
                //QuoteId = dto.QuoteId,
				FromCurrency = dto.FromCurrency,
				ToCurrency = dto.ToCurrency,

			};

			await _repo.CreateAsync(remittance);

			return new CreateRemittanceResponseDto
			{
				RemitId = remittance.RemitId,
				CustomerId = remittance.CustomerId,
				BeneficiaryId = remittance.BeneficiaryId,  // Added!
				Amount = remittance.SendAmount,
				FromCurrency = remittance.FromCurrency,    // Added! (To fix the null)
				ToCurrency = remittance.ToCurrency,        // Added! (To fix the null)
				Status = remittance.Status.ToString(),
				CreatedDate = remittance.CreatedDate,      // Added! (To fix the 0001-01-01)
				IsDeleted = remittance.IsDeleted
     
			};
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

            // Fetch remittance
            var remittance = await _repo.GetByIdAsync(remitId);
            if (remittance == null)
                throw new Exception("Remittance not found.");

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
                CheckedDate = DateTimeOffset.UtcNow
            });

            // Limit rule (example – real logic later)
            validations.Add(new RemitValidation
            {
                ValidationId = Guid.NewGuid(),
                RemitId = remittance.RemitId,
                RuleName = ValidationRuleName.Limit,
                Result = ValidationResult.Pass,
                Message = string.Empty,
                CheckedDate = DateTimeOffset.UtcNow
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
                    CheckedDate = DateTimeOffset.UtcNow
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
                    CheckedDate = DateTimeOffset.UtcNow
                });
            }

            // Save validation records via validation repository
            await _validationRepo.AddRangeAsync(validations);

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
            existing.CheckedDate = dto.CheckedDate;
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

        public async Task<IEnumerable<CreateRemittanceResponseDto>> GetAllAsync()
        {
            var remittances = await _repo.GetAllAsync();
            return remittances.Select(r => new CreateRemittanceResponseDto
            {
                RemitId = r.RemitId,
                CustomerId = r.CustomerId,
                Amount = r.SendAmount,
                Status = r.Status.ToString(),
                IsDeleted = r.IsDeleted
            });
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
                IsDeleted = v.IsDeleted
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
                IsDeleted = validation.IsDeleted
            };
        }
	}
}