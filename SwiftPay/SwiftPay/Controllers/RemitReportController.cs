using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using SwiftPay.Services.Interfaces;
using SwiftPay.DTOs.RemitReportDTO;
using SwiftPay.Domain.Remittance.Entities;

namespace SwiftPay.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RemitReportController : ControllerBase
    {
        private readonly IRemitReportService _service;

        public RemitReportController(IRemitReportService service)
        {
            _service = service;
        }

        /// <summary>
        /// Create a new remit report
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(RemitReport), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Create([FromBody] CreateRemitReportDto dto)
        {
            if (dto == null) return BadRequest(new { message = "Request body is required." });
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var created = await _service.CreateAsync(dto);
                return Ok(new { message = "RemitReport created successfully.", data = created });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while creating the remit report.", error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(RemitReport), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var item = await _service.GetByIdAsync(id);
                if (item == null) return NotFound(new { message = $"RemitReport with ID {id} not found." });
                return Ok(new { message = "RemitReport retrieved successfully.", data = item });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while retrieving the remit report.", error = ex.Message });
            }
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<RemitReport>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var list = await _service.GetAllAsync();
                return Ok(new { message = "RemitReports retrieved successfully.", data = list });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while retrieving remit reports.", error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        [ProducesResponseType(typeof(RemitReport), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Update(int id, [FromBody] CreateRemitReportDto dto)
        {
            if (dto == null) return BadRequest(new { message = "Request body is required." });
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var updated = await _service.UpdateAsync(id, dto);
                return Ok(new { message = "RemitReport updated successfully.", data = updated });
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("not found")) return NotFound(new { message = ex.Message });
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while updating the remit report.", error = ex.Message });
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
                if (!result) return NotFound(new { message = $"RemitReport with ID {id} not found." });
                return Ok(new { message = "RemitReport deleted successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while deleting the remit report.", error = ex.Message });
            }
        }
    }
}
