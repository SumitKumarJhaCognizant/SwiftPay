using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SwiftPay.Constants.Enums;
using SwiftPay.DTOs.RemittanceDTO;
using SwiftPay.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SwiftPay.Controllers
{
	[Authorize]
	[ApiController]
	[Route("api/[controller]")]
	public class DocumentController : ControllerBase
	{
		private readonly IDocumentService _documentService;

		public DocumentController(IDocumentService documentService)
		{
			_documentService = documentService;
		}

		[HttpPost]
		[Authorize(Roles = "Customer,Agent,Compliance,Admin")]
		public async Task<IActionResult> Create([FromBody] CreateDocumentDto dto)
		{
			try
			{
				if (dto == null) return BadRequest(new { message = "Body is required." });
				if (string.IsNullOrWhiteSpace(dto.FileURI)) return BadRequest(new { message = "FileURI is required." });

				var created = await _documentService.CreateAsync(dto);
				return Ok(new { message = "Document created successfully.", data = created });
			}
			catch (Exception ex)
			{
				return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Error creating document.", error = ex.Message });
			}
		}

		[HttpGet("{id}")]
		[Authorize(Roles = "Customer,Agent,Compliance,Admin")]
		public async Task<IActionResult> GetById(int id)
		{
			try
			{
				var doc = await _documentService.GetByIdAsync(id);
				if (doc == null) return NotFound(new { message = "Document not found." });
				return Ok(doc);
			}
			catch (Exception ex)
			{
				return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Error retrieving document.", error = ex.Message });
			}
		}

		[HttpGet]
		[Authorize(Roles = "Agent,Compliance,Admin")]
		public async Task<IActionResult> GetByRemitId([FromQuery] int remitId)
		{
			try
			{
				if (remitId <= 0) return BadRequest(new { message = "remitId is required." });

				var list = await _documentService.GetByRemitIdAsync(remitId);
				return Ok(list);
			}
			catch (Exception ex)
			{
				return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Error retrieving documents.", error = ex.Message });
			}
		}

		[HttpPut("{id}")]
		[Authorize(Roles = "Compliance,Admin")]
		public async Task<IActionResult> Update(int id, [FromBody] UpdateDocumentDto dto)
		{
			try
			{
				if (id != dto.DocumentId) return BadRequest(new { message = "ID mismatch." });

				await _documentService.UpdateAsync(dto);
				return NoContent();
			}
			catch (KeyNotFoundException)
			{
				return NotFound(new { message = "Document not found." });
			}
			catch (Exception ex)
			{
				return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Update failed.", error = ex.Message });
			}
		}

		[HttpDelete("{id}")]
		[Authorize(Roles = "Admin,Compliance")]
		public async Task<IActionResult> Delete(int id)
		{
			try
			{
				var document = await _documentService.GetByIdAsync(id);
				if (document == null) return NotFound(new { message = "Document not found." });

				await _documentService.DeleteAsync(id);
				return NoContent();
			}
			catch (Exception ex)
			{
				return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Deletion failed.", error = ex.Message });
			}
		}

		[HttpPatch("{id}/verify")]
		[Authorize(Roles = "Compliance,Admin")]
		public async Task<IActionResult> VerifyDocument(int id, [FromBody] VerificationStatus status)
		{
			try
			{
				if (!Enum.IsDefined(typeof(VerificationStatus), status))
					return BadRequest(new { message = "Invalid verification status." });

				await _documentService.UpdateVerificationStatusAsync(id, status);
				return NoContent();
			}
			catch (KeyNotFoundException)
			{
				return NotFound(new { message = "Document not found." });
			}
			catch (Exception ex)
			{
				return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Verification update failed.", error = ex.Message });
			}
		}
	}
}