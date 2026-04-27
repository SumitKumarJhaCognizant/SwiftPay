using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SwiftPay.Domain.Remittance.Entities;
using SwiftPay.DTOs.RefundRefDTO;
using SwiftPay.Services.Interfaces;
using System;
using System.Collections.Generic;

namespace SwiftPay.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("api/[controller]")]
    [ApiController]
    public class RefundRefController : ControllerBase
    {
        private readonly IRefundRefService _service;

        public RefundRefController(IRefundRefService service)
        {
            _service = service;
        }

 

        /// <summary>
        /// Create a new refund reference
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(RefundRef), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Create([FromBody] CreateRefundRefDto dto)
        {
            if (dto == null) return BadRequest(new { message = "Request body is required." });
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var created = await _service.CreateAsync(dto);
                return Ok(new { message = "RefundRef created successfully.", data = created });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while creating the refund ref.", error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(RefundRef), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var item = await _service.GetByIdAsync(id);
                if (item == null) return NotFound(new { message = $"RefundRef with ID {id} not found." });
                return Ok(new { message = "RefundRef retrieved successfully.", data = item });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while retrieving the refund ref.", error = ex.Message });
            }
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<RefundRef>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var list = await _service.GetAllAsync();
                return Ok(new { message = "RefundRefs retrieved successfully.", data = list });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while retrieving refund refs.", error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        [ProducesResponseType(typeof(RefundRef), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Update(int id, [FromBody] CreateRefundRefDto dto)
        {
            if (dto == null) return BadRequest(new { message = "Request body is required." });
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var updated = await _service.UpdateAsync(id, dto);
                return Ok(new { message = "RefundRef updated successfully.", data = updated });
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("not found")) return NotFound(new { message = ex.Message });
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while updating the refund ref.", error = ex.Message });
            }
        }

        /// <summary>
        /// Update refund ref status
        /// </summary>
        [Authorize(Roles = "Admin,Treasury")]
        [HttpPatch("{id}/status")]
        [ProducesResponseType(typeof(RefundRef), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> PatchStatus(int id, [FromBody] Constants.Enums.RefundStatus status)
        {
            try
            {
                var updated = await _service.UpdateStatusAsync(id, status);
                return Ok(new { message = "RefundRef status updated successfully.", data = updated });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while updating refund ref status.", error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var result = await _service.DeleteAsync(id);
                if (!result) return NotFound(new { message = $"RefundRef with ID {id} not found." });
                return Ok(new { message = "RefundRef deleted successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while deleting the refund ref.", error = ex.Message });
            }
        }
    }
}
