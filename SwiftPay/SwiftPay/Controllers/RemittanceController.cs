using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using SwiftPay.Services.Interfaces;
using SwiftPay.DTOs.RemittanceDTO;
using SwiftPay.DTOs.UserCustomerDTO;
using SwiftPay.DTOs.RefundRefDTO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Formats.Asn1;
using SwiftPay.Constants.Enums;

namespace SwiftPay.Controllers
{
	[Authorize] // 1. Global requirement: Must be authenticated
	[ApiController]
	[Route("api/[controller]")]
	public class RemittancesController : ControllerBase
	{
		private readonly IRemittanceService _remittanceService;
		private readonly SwiftPay.Services.Interfaces.IDocumentService _documentService;
		private readonly INotificationAlertService _notificationService;
		private readonly ICustomerService _customerService;
		private readonly IRefundRefService _refundService;

		public RemittancesController(
			IRemittanceService remittanceService,
			SwiftPay.Services.Interfaces.IDocumentService documentService,
			INotificationAlertService notificationService,
			ICustomerService customerService,
			IRefundRefService refundService)
		{
			_remittanceService = remittanceService;
			_documentService = documentService;
			_notificationService = notificationService;
			_customerService = customerService;
			_refundService = refundService;
		}

		// Resolve the customer's user id for a given remit, used as the notification recipient.
		private async Task<int?> ResolveNotifyUserIdAsync(int remitId)
		{
			try
			{
				var remit = await _remittanceService.GetByIdAsync(remitId);
				if (remit == null) return null;
				var customer = await _customerService.GetByIdAsync(remit.CustomerId);
				return customer?.UserID;
			}
			catch { return null; }
		}

		// Best-effort fire-and-forget audit notification. Never block the main flow on this.
		private async Task TryNotifyAsync(int? userId, int? remitId, NotificationCategory category, string message)
		{
			if (userId == null || userId <= 0) return;
			try
			{
				await _notificationService.CreateAsync(new CreateNotificationDto
				{
					UserID = userId.Value,
					RemitID = remitId,
					Message = message,
					Category = category,
				});
			}
			catch
			{
				// Don't fail the parent operation if notification logging fails.
			}
		}

		// --- INITIATION PHASE (Role: Customer, Agent) ---

		/// <summary>
		/// Creates a new remittance request in Draft state.
		/// </summary>
		[HttpPost]
		[Authorize(Roles = "Customer,Agent,Admin")] // Only initiators can start a remit
		[ProducesResponseType(typeof(CreateRemittanceResponseDto), StatusCodes.Status201Created)]
		public async Task<IActionResult> Create([FromBody] CreateRemittanceDto dto)
		{
			try
			{
				var createdRemittance = await _remittanceService.CreateAsync(dto);

				// Resolve the linked user so we can attach the notification to the customer's user account.
				int? notifyUserId = null;
				try
				{
					var customer = await _customerService.GetByIdAsync(dto.CustomerId);
					if (customer != null) notifyUserId = customer.UserID;
				}
				catch { /* ignore */ }

				// Fall back to caller's JWT subject if customer lookup failed.
				if (notifyUserId == null)
				{
					var sub = User.FindFirstValue(ClaimTypes.NameIdentifier);
					if (int.TryParse(sub, out var fromJwt)) notifyUserId = fromJwt;
				}

				await TryNotifyAsync(
					notifyUserId,
					createdRemittance.RemitId,
					NotificationCategory.Routing,
					$"Remittance #{createdRemittance.RemitId} submitted: {dto.FromCurrency} {dto.SendAmount:0.00} → {dto.ToCurrency}. Status: Draft.");

				return Ok(new { message = "Remittance created successfully.", data = createdRemittance });
			}
			catch (Exception ex)
			{
				return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to create remittance.", error = ex.Message });
			}
		}

		/// <summary>
		/// Upload supporting document and advance state to PendingCompliance.
		/// </summary>
		[HttpPost("{remitId:int}/documents")]
		[Authorize(Roles = "Customer,Agent,Admin")] // The person sending the money provides the proof
		public async Task<IActionResult> UploadDocument(int remitId, [FromBody] CreateDocumentDto dto)
		{
			try
			{
				if (dto == null || dto.RemitId != remitId) return BadRequest(new { message = "Invalid DTO or RemitId mismatch." });

				var created = await _documentService.CreateAsync(dto);
				await _remittanceService.MarkPendingComplianceAsync(remitId);

				var sub = User.FindFirstValue(ClaimTypes.NameIdentifier);
				int.TryParse(sub, out var fromJwt);
				await TryNotifyAsync(fromJwt, remitId, NotificationCategory.Compliance,
					$"Document uploaded for remittance #{remitId}. Status moved to PendingCompliance.");

				return Ok(new { message = "Document uploaded.", data = new { documentId = created.DocumentId, status = "PendingCompliance" } });
			}
			catch (Exception ex)
			{
				return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to upload document.", error = ex.Message });
			}
		}

		// --- VALIDATION & CORRIDOR RULES (Role: Agent, Admin, System) ---

		/// <summary>
		/// Runs validation rules (Corridor/Velocity checks).
		/// </summary>
		[HttpPost("{remitId:int}/validate")]
		[Authorize(Roles = "Admin,Agent")] // Usually triggered by the app/system after Draft
		public async Task<IActionResult> Validate(int remitId)
		{
			try
			{
				var response = await _remittanceService.ValidateAsync(remitId);
				if (response.Status != "Validated") return UnprocessableEntity(response);
				return Ok(response);
			}
			catch (Exception ex)
			{
				return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Validation failed.", error = ex.Message });
			}
		}

		// --- AGENT REVIEW PHASE: confirm or reject pending remittances ---

		/// <summary>
		/// Agent / Admin approves a customer-submitted remittance: marks it Paid and
		/// notifies the customer that their payment has completed.
		/// </summary>
		[HttpPost("{remitId:int}/approve")]
		[Authorize(Roles = "Agent,Admin")]
		public async Task<IActionResult> Approve(int remitId)
		{
			try
			{
				var existing = await _remittanceService.GetByIdAsync(remitId);
				if (existing == null) return NotFound(new { message = "Remittance not found." });

				if (existing.Status == "Paid")
					return Conflict(new { message = "Remittance is already marked Paid." });
				if (existing.Status == "Cancelled" || existing.Status == "Refunded")
					return Conflict(new { message = $"Remittance is {existing.Status} and cannot be approved." });

				await _remittanceService.UpdateVerificationStatusByRemitIdAsync(remitId, RemittanceRequestStatus.Paid);

				var notifyUserId = await ResolveNotifyUserIdAsync(remitId);
				await TryNotifyAsync(notifyUserId, remitId, NotificationCategory.Payout,
					$"Your payment for remittance #{remitId} has been completed successfully.");

				var updated = await _remittanceService.GetByIdAsync(remitId);
				return Ok(new { message = "Remittance approved and marked as Paid.", data = updated });
			}
			catch (Exception ex)
			{
				return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Approval failed.", error = ex.Message });
			}
		}

		/// <summary>
		/// Agent / Admin rejects a remittance (e.g. suspicious activity). Marks it Cancelled,
		/// auto-creates a RefundRef row for the full SendAmount, and notifies the customer.
		/// </summary>
		[HttpPost("{remitId:int}/reject")]
		[Authorize(Roles = "Agent,Admin")]
		public async Task<IActionResult> Reject(int remitId, [FromBody] RejectRemittanceDto? body)
		{
			try
			{
				var reason = body?.Reason?.Trim();
				if (string.IsNullOrWhiteSpace(reason))
					return BadRequest(new { message = "A rejection reason is required." });

				var existing = await _remittanceService.GetByIdAsync(remitId);
				if (existing == null) return NotFound(new { message = "Remittance not found." });
				if (existing.Status == "Paid")
					return Conflict(new { message = "Cannot reject a remittance that has already been paid." });
				if (existing.Status == "Cancelled" || existing.Status == "Refunded")
					return Conflict(new { message = $"Remittance is already {existing.Status}." });

				// 1. Move the remittance to Cancelled.
				await _remittanceService.UpdateVerificationStatusByRemitIdAsync(remitId, RemittanceRequestStatus.Cancelled);

				// 2. Log a refund (Phase-1 reference — Method=Original, Status=Initiated by default).
				int? refundId = null;
				try
				{
					var refund = await _refundService.CreateAsync(new CreateRefundRefDto
					{
						RemitId = remitId,
						Amount = existing.Amount,
						Method = RefundMethod.Original,
					});
					refundId = refund?.RefundID;
				}
				catch { /* don't block rejection if refund write fails */ }

				// 3. Notify the customer.
				var notifyUserId = await ResolveNotifyUserIdAsync(remitId);
				var refundLine = refundId.HasValue ? $" Refund #{refundId} initiated for {existing.Amount:0.00}." : string.Empty;
				await TryNotifyAsync(notifyUserId, remitId, NotificationCategory.Refund,
					$"Your remittance #{remitId} was rejected by the agent. Reason: {reason}.{refundLine}");

				var updated = await _remittanceService.GetByIdAsync(remitId);
				return Ok(new { message = "Remittance rejected and refund logged.", data = new { remit = updated, refundId } });
			}
			catch (Exception ex)
			{
				return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Rejection failed.", error = ex.Message });
			}
		}

		// --- COMPLIANCE & ADMIN MANAGEMENT (Role: Compliance, Admin) ---

		/// <summary>
		/// Retrieves all remittances (Hidden from Customers for security).
		/// </summary>
		[HttpGet]
		[Authorize(Roles = "Admin,Compliance,Agent,Ops")]
		public async Task<IActionResult> GetAll()
		{
			var remittances = await _remittanceService.GetAllAsync();
			return Ok(remittances.Where(r => !r.IsDeleted).ToList());
		}

		/// <summary>
		/// Update a validation record (Role: Compliance Officers).
		/// </summary>
		[HttpPut("validations/{validationId}")]
		[Authorize(Roles = "Compliance,Admin")]
		public async Task<IActionResult> UpdateValidation(Guid validationId, [FromBody] RemitValidationDto dto)
		{
			if (validationId != dto.ValidationId) return BadRequest(new { message = "ValidationId mismatch." });
			await _remittanceService.UpdateValidationAsync(dto);
			return Ok(new { message = "Validation updated." });
		}

		/// <summary>
		/// Soft delete a remittance.
		/// </summary>
		[HttpDelete("{remitId:int}")]
		[Authorize(Roles = "Admin")] // High-level restriction
		public async Task<IActionResult> Delete(int remitId)
		{
			await _remittanceService.SoftDeleteAsync(remitId);
			return NoContent();
		}

		/// <summary>
		/// Update the verification status of a remittance by RemitId.
		/// </summary>
		[HttpPut("{remitId:int}/verification-status")]
		[Authorize(Roles = "Admin,Compliance")]
		public async Task<IActionResult> UpdateVerificationStatus(int remitId, [FromBody] RemittanceRequestStatus status)
		{
			try
			{
				await _remittanceService.UpdateVerificationStatusByRemitIdAsync(remitId, status);

				// Find the originating customer's user to log the audit notification.
				int? notifyUserId = null;
				try
				{
					var remit = await _remittanceService.GetByIdAsync(remitId);
					if (remit != null)
					{
						var customer = await _customerService.GetByIdAsync(remit.CustomerId);
						if (customer != null) notifyUserId = customer.UserID;
					}
				}
				catch { /* ignore */ }

				await TryNotifyAsync(notifyUserId, remitId, NotificationCategory.Compliance,
					$"Remittance #{remitId} status updated to {status}.");

				return Ok(new { message = "Verification status updated successfully." });
			}
			catch (KeyNotFoundException)
			{
				return NotFound(new { message = "Remittance not found or already deleted." });
			}
			catch (Exception ex)
			{
				return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to update verification status.", error = ex.Message });
			}
		}

		// --- GENERAL ACCESS (Role: All Authenticated) ---

		[HttpGet("{remitId:int}")]
		[Authorize(Roles = "Customer,Agent,Compliance,Admin")]
		public async Task<IActionResult> GetById(int remitId)
		{
			var result = await _remittanceService.GetByIdAsync(remitId);
			return result == null ? NotFound() : Ok(result);
		}

		[HttpGet("{remitId:int}/validations")]
		[Authorize(Roles = "Customer,Agent,Compliance,Admin")]
		public async Task<IActionResult> GetValidations(int remitId)
		{
			// Empty list (not 404) when no validations exist yet — the resource is the collection,
			// which legitimately is empty for a freshly-created remit.
			var validations = await _remittanceService.GetValidationsAsync(remitId);
			return Ok(validations ?? Enumerable.Empty<RemitValidationDto>());
		}


		}

	}
