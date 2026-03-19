using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using SwiftPay.Services.Interfaces;
using SwiftPay.DTOs.AmendmentDTO;
using SwiftPay.Domain.Remittance.Entities;

namespace SwiftPay.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AmendmentController : ControllerBase
    {
        private readonly IAmendmentService _amendmentService;

        public AmendmentController(IAmendmentService amendmentService)
        {
            _amendmentService = amendmentService;
        }

        /// <summary>
        /// Create a new amendment request
        /// </summary>
        /// <param name="dto">Amendment creation data</param>
        /// <returns>Created amendment object</returns>
        /// <response code="200">Amendment created successfully</response>
        /// <response code="400">Invalid request data</response>
        /// <response code="500">Server error</response>
        [HttpPost]
        [ProducesResponseType(typeof(Amendment), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Create([FromBody] CreateAmendmentDto dto)
        {
            if (dto == null)
                return BadRequest(new { message = "Request body is required." });

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var created = await _amendmentService.CreateAsync(dto);
                return Ok(new { message = "Amendment created successfully.", data = created });
            }
            catch (KeyNotFoundException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while creating the amendment.", error = ex.Message });
            }
        }

        /// <summary>
        /// Get amendment by ID
        /// </summary>
        /// <param name="id">Amendment ID</param>
        /// <returns>Amendment object</returns>
        /// <response code="200">Amendment found</response>
        /// <response code="404">Amendment not found</response>
        /// <response code="500">Server error</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(Amendment), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var item = await _amendmentService.GetByIdAsync(id);
                if (item == null)
                    return NotFound(new { message = $"Amendment with ID {id} not found." });

                return Ok(new { message = "Amendment retrieved successfully.", data = item });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while retrieving the amendment.", error = ex.Message });
            }
        }

        /// <summary>
        /// Get all amendments
        /// </summary>
        /// <returns>List of amendments</returns>
        /// <response code="200">Amendments retrieved successfully</response>
        /// <response code="500">Server error</response>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<Amendment>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var list = await _amendmentService.GetAllAsync();
                return Ok(new { message = "Amendments retrieved successfully.", data = list });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while retrieving amendments.", error = ex.Message });
            }
        }

        /// <summary>
        /// Update an amendment
        /// </summary>
        /// <param name="id">Amendment ID</param>
        /// <param name="dto">Amendment update data</param>
        /// <returns>Updated amendment object</returns>
        /// <response code="200">Amendment updated successfully</response>
        /// <response code="404">Amendment not found</response>
        /// <response code="400">Invalid request data</response>
        /// <response code="500">Server error</response>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(Amendment), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Update(int id, [FromBody] CreateAmendmentDto dto)
        {
            if (dto == null)
                return BadRequest(new { message = "Request body is required." });

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var updated = await _amendmentService.UpdateAsync(id, dto);
                return Ok(new { message = "Amendment updated successfully.", data = updated });
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("not found"))
                    return NotFound(new { message = ex.Message });

                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while updating the amendment.", error = ex.Message });
            }
        }

        /// <summary>
        /// Delete (soft delete) an amendment
        /// </summary>
        /// <param name="id">Amendment ID</param>
        /// <returns>Success or failure message</returns>
        /// <response code="200">Amendment deleted successfully</response>
        /// <response code="404">Amendment not found</response>
        /// <response code="500">Server error</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var result = await _amendmentService.DeleteAsync(id);
                if (!result)
                    return NotFound(new { message = $"Amendment with ID {id} not found." });

                return Ok(new { message = "Amendment deleted successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while deleting the amendment.", error = ex.Message });
            }
        }
    }
}
