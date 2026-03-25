using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using SwiftPay.Services.Interfaces;
using SwiftPay.DTOs.RemittanceDTO;
namespace SwiftPay.Controllers
{
	[ApiController]
	[Route("api/remittances")]
	public class RemittancesController : ControllerBase
	{
		private readonly IRemittanceService _remittanceService;
		private readonly SwiftPay.Services.Interfaces.IDocumentService _documentService;

		public RemittancesController(IRemittanceService remittanceService, SwiftPay.Services.Interfaces.IDocumentService documentService)
		{
			_remittanceService = remittanceService;
			_documentService = documentService;
		}

		/// <summary>
		/// Upload supporting document for a remit and advance state to PendingCompliance.
		/// </summary>
        [HttpPost("{remitId:int}/documents")]
		[ProducesResponseType(StatusCodes.Status201Created)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UploadDocument(int remitId, [FromBody] CreateDocumentDto dto)
		{
			try
			{
				if (dto == null) return BadRequest(new { message = "Request body is required." });
                if (dto.RemitId != remitId)
                    return BadRequest(new { message = "RemitId mismatch." });

				var created = await _documentService.CreateAsync(dto);
				await _remittanceService.MarkPendingComplianceAsync(remitId);

				return CreatedAtAction(nameof(GetById), new { remitId = remitId }, new { documentId = created.DocumentId, status = "PendingCompliance" });
			}
			catch (Exception ex)
			{
				return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to upload document.", error = ex.Message });
			}
		}

		/// <summary>
		/// Cancel a remittance (phase-1 mock refund).
		/// </summary>
        [HttpPost("{remitId:int}/cancel")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Cancel(int remitId, [FromBody] dynamic body)
		{
			try
			{
				string reason = body?.cancellationReason ?? string.Empty;
				var refundRef = await _remittanceService.CancelAsync(remitId, reason);
			
				return Ok(new { message = "Remittance cancelled.", refundReference = refundRef });
			}
			catch (InvalidOperationException ex)
			{
				return BadRequest(new { message = ex.Message });
			}
			catch (Exception ex)
			{
				return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to cancel remittance.", error = ex.Message });
			}
		}

		/// <summary>
		/// Update a remittance (replace fields).
		/// </summary>
        [HttpPut("{remitId:int}")]
		[ProducesResponseType(StatusCodes.Status204NoContent)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Update(int remitId, [FromBody] UpdateRemittanceDto dto)
		{
			try
			{
                await _remittanceService.UpdateAsync(remitId, dto);
				return NoContent();
			}
			catch (Exception ex)
			{
				return StatusCode(
					StatusCodes.Status500InternalServerError,
					new { message = "Failed to update remittance.", error = ex.Message }
				);
			}
		}

		/// <summary>
		/// Delete (soft) a remittance.
		/// </summary>
        [HttpDelete("{remitId:int}")]
		[ProducesResponseType(StatusCodes.Status204NoContent)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int remitId)
		{
			try
			{
                await _remittanceService.SoftDeleteAsync(remitId);
                return NoContent();
			}
			catch (Exception ex)
			{
				return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to delete remittance.", error = ex.Message });
			}
		}

		/// <summary>
		/// Update a validation record for a remittance.
		/// </summary>
        [HttpPut("{remitId:int}/validations/{validationId}")]
		[ProducesResponseType(StatusCodes.Status204NoContent)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateValidation(int remitId, Guid validationId, [FromBody] RemitValidationDto dto)
		{
			try
			{
                if (validationId != dto.ValidationId || remitId != dto.RemitId)
                    return BadRequest(new { message = "RemitId or ValidationId mismatch." });

				await _remittanceService.UpdateValidationAsync(dto);
				return NoContent();
			}
			catch (Exception ex)
			{
				return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to update validation.", error = ex.Message });
			}
		}

		/// <summary>
		/// Delete a validation record for a remittance.
		/// </summary>
        [HttpDelete("{remitId:int}/validations/{validationId}")]
		[ProducesResponseType(StatusCodes.Status204NoContent)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteValidation(int remitId, Guid validationId)
		{
			try
			{
				// optional: ensure validation belongs to remitId
				await _remittanceService.DeleteValidationAsync(validationId);
				return NoContent();
			}
			catch (Exception ex)
			{
				return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to delete validation.", error = ex.Message });
			}
		}

		/// <summary>
		/// Creates a new remittance request in Draft state.
		/// </summary>
		[HttpPost]
		[ProducesResponseType(typeof(CreateRemittanceResponseDto), StatusCodes.Status201Created)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
		public async Task<IActionResult> Create([FromBody] CreateRemittanceDto dto)
		{
			try
			{
				var createdRemittance = await _remittanceService.CreateAsync(dto);

				return CreatedAtAction(
					nameof(GetById),
					new { remitId = createdRemittance.RemitId },
					createdRemittance
				);
			}
			catch (Exception ex)
			{
				return StatusCode(
					StatusCodes.Status500InternalServerError,
					new { message = "Failed to create remittance.", error = ex.Message , innerError = ex.InnerException?.Message }
					
				);
			}
		}

		/// <summary>
		/// Retrieves remittance details by ID.
		/// </summary>
        [HttpGet("{remitId:int}")]
		[ProducesResponseType(typeof(CreateRemittanceResponseDto), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetById(int remitId)
		{
			try
			{
                var result = await _remittanceService.GetByIdAsync(remitId);

				if (result == null)
					return NotFound(new { message = "Remittance not found." });

				return Ok(result);
			}
			catch (Exception ex)
			{
				return StatusCode(
					StatusCodes.Status500InternalServerError,
					new
					{
						message = "Failed to retrieve remittance.",
						error = ex.Message
					}
				);
			}
		}


		/// <summary>
		/// Runs validation rules for a remittance.
		/// </summary>
        [HttpPost("{remitId:int}/validate")]
		[ProducesResponseType(typeof(ValidateRemittanceResponseDto), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Validate(int remitId)
		{
			try
			{
                var response = await _remittanceService.ValidateAsync(remitId);

				// If validation failed, service should return status Draft
				if (response.Status != "Validated")
				{
					return UnprocessableEntity(response);
				}

				return Ok(response);
			}
			catch (Exception ex)
			{
				return StatusCode(
					StatusCodes.Status500InternalServerError,
					new
					{
						message = "Failed to validate remittance.",
						error = ex.Message
					}
				);
			}
		}

		/// <summary>
		/// Retrieves validation results for a remittance.
		/// </summary>
        [HttpGet("{remitId:int}/validations")]
		[ProducesResponseType(typeof(List<RemitValidationDto>), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetValidations(int remitId)
		{
			try
			{
                var validations = await _remittanceService.GetValidationsAsync(remitId);

				if (validations == null || !validations.Any())
					return NotFound(new { message = "No validation records found." });

				return Ok(validations);
			}
			catch (Exception ex)
			{
				return StatusCode(
					StatusCodes.Status500InternalServerError,
					new
					{
						message = "Failed to retrieve validation results.",
						error = ex.Message
					}
				);
			}
		}

		/// <summary>
		/// Retrieves all remittances (excluding soft-deleted).
		/// </summary>
        [HttpGet]
		[ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
		{
			var remittances = await _remittanceService.GetAllAsync();
			var activeRemittances = remittances.Where(r => !r.IsDeleted).ToList();
			return Ok(activeRemittances);
		}

        [HttpGet("validations")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetValidationsByRemitId([FromQuery] int remitId)
        {
            if (remitId <= 0)
                return BadRequest(new { message = "remitId query parameter is required and must be a positive integer." });

            var validations = await _remittanceService.GetValidationsByRemitIdAsync(remitId);
            var activeValidations = validations.Where(v => !v.IsDeleted).ToList();
            return Ok(activeValidations);
        }

        [HttpPut("validations/{validationId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateValidation(Guid validationId, [FromBody] RemitValidationDto dto)
        {
            if (validationId != dto.ValidationId)
                return BadRequest(new { message = "ValidationId mismatch." });

            await _remittanceService.UpdateValidationAsync(dto);
            return Ok(new { message = "Validation updated successfully." });
        }

        [HttpDelete("validations/{validationId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> DeleteValidation(Guid validationId)
        {
            var validation = await _remittanceService.GetValidationByIdAsync(validationId);
            if (validation == null || validation.IsDeleted)
                return NotFound(new { message = "Validation not found or already deleted." });

            validation.IsDeleted = true;
            await _remittanceService.UpdateValidationAsync(validation);
            return NoContent();
        }
	}
}