using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using SwiftPay.Services.Interfaces;
using SwiftPay.DTOs.RemittanceDTO;
using System.Linq;
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

		public RemittancesController(IRemittanceService remittanceService, SwiftPay.Services.Interfaces.IDocumentService documentService)
		{
			_remittanceService = remittanceService;
			_documentService = documentService;
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
				return CreatedAtAction(nameof(GetById), new { remitId = createdRemittance.RemitId }, createdRemittance);
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

				return CreatedAtAction(nameof(GetById), new { remitId = remitId }, new { documentId = created.DocumentId, status = "PendingCompliance" });
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

		// --- COMPLIANCE & ADMIN MANAGEMENT (Role: Compliance, Admin) ---

		/// <summary>
		/// Retrieves all remittances (Hidden from Customers for security).
		/// </summary>
		[HttpGet]
		[Authorize(Roles = "Admin,Compliance")]
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
			var validations = await _remittanceService.GetValidationsAsync(remitId);
			return (validations == null || !validations.Any()) ? NotFound() : Ok(validations);
		}


		}

	}
