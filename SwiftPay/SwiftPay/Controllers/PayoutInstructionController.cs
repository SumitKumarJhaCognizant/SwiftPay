using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SwiftPay.Constants.Enums;
using SwiftPay.DTOs.PayoutDTO;
using SwiftPay.Services.Interfaces;

namespace SwiftPay.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	[Authorize(Roles = "Ops,Admin")]
	public class PayoutInstructionController : ControllerBase
	{
		private readonly IPayoutInstructionService _service;

		public PayoutInstructionController(IPayoutInstructionService service)
		{
			_service = service;
		}

		[HttpPost]
		public async Task<IActionResult> Create([FromBody] CreatePayoutInstructionDto dto)
		{
			var result = await _service.CreateInstructionAsync(dto);
			return CreatedAtAction(nameof(GetById), new { id = result.InstructionId }, result);
		}

		[HttpGet("{id}")]
		public async Task<IActionResult> GetById(string id)
		{
			var result = await _service.GetByIdAsync(id);
			return result != null ? Ok(result) : NotFound();
		}

		[HttpGet]
		public async Task<IActionResult> GetAll()
		{
			var result = await _service.GetAllAsync();
			return Ok(new { message = "Payout instructions retrieved successfully.", data = result });
		}

		[HttpPut("{id}")]
		public async Task<IActionResult> Update(string id, [FromBody] CreatePayoutInstructionDto dto)
		{
			var success = await _service.UpdateAsync(id, dto);
			return success ? NoContent() : NotFound();
		}

		[HttpPatch("{id}/status")]
		public async Task<IActionResult> UpdateStatus(string id, [FromBody] UpdatePayoutStatusDto dto)
		{
			var success = await _service.UpdateStatusAsync(id, dto.Status, dto.AckRef ?? string.Empty);
			if (!success) return NotFound(new { message = "Payout instruction not found." });
			return Ok(new { message = $"Status updated to {dto.Status}." });
		}

		[HttpDelete("{id}")]
		public async Task<IActionResult> Delete(string id)
		{
			var success = await _service.DeleteAsync(id);
			return success ? NoContent() : NotFound();
		}
	}
}