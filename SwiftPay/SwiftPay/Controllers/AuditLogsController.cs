using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SwiftPay.Services.Interfaces;
using SwiftPay.DTOs.UserCustomerDTO;

namespace SwiftPay.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,Ops,Compliance")]
    public class AuditLogsController : ControllerBase
    {
        private readonly IAuditLogService _service;

        public AuditLogsController(IAuditLogService service)
        {
            _service = service;
        }

        /// <summary>
        /// Get audit logs with advanced filtering by UserId, Resource, and DateRange
        /// </summary>
        /// <param name="filter">Filter criteria with pagination</param>
        /// <returns>Filtered audit logs with pagination metadata</returns>
        /// <response code="200">Audit logs retrieved successfully</response>
        /// <response code="400">Invalid filter parameters</response>
        /// <response code="500">Server error</response>
        [HttpGet("filter")]
        [ProducesResponseType(typeof(AuditLogListDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAuditLogsFiltered([FromQuery] AuditLogFilterDto filter)
        {
            try
            {
                // Validate custom business logic (date range check)
                if (!filter.IsValid(out var error))
                    return BadRequest(new { message = error });

                var response = await _service.GetFilteredAsync(
                    filter.UserId,
                    filter.Resource,
                    filter.StartDate,
                    filter.EndDate,
                    filter.PageNumber,
                    filter.PageSize);

                return Ok(new { message = "Audit logs retrieved successfully.", data = response });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving audit logs.", error = ex.Message });
            }
        }

        /// <summary>
        /// Get audit log by ID
        /// </summary>
        /// <param name="auditId">Audit ID</param>
        /// <returns>Audit log object</returns>
        /// <response code="200">Audit log found</response>
        /// <response code="404">Audit log not found</response>
        /// <response code="500">Server error</response>
        [HttpGet("{auditId}")]
        [ProducesResponseType(typeof(GetAuditLogDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetById(int auditId)
        {
            try
            {
                var dto = await _service.GetByIdAsync(auditId);
                if (dto == null)
                    return NotFound(new { message = $"Audit log with ID {auditId} not found." });

                return Ok(new { message = "Audit log retrieved successfully.", data = dto });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving the audit log.", error = ex.Message });
            }
        }

        /// <summary>
        /// Get audit logs by User ID
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>List of audit logs for the user</returns>
        /// <response code="200">Audit logs retrieved successfully</response>
        /// <response code="500">Server error</response>
        [HttpGet("user/{userId}")]
        [ProducesResponseType(typeof(IEnumerable<GetAuditLogDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetByUserId(int userId)
        {
            try
            {
                var dtos = await _service.GetByUserIdAsync(userId);
                return Ok(new { message = "Audit logs retrieved successfully.", data = dtos });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving audit logs.", error = ex.Message });
            }
        }

        /// <summary>
        /// Get audit logs by Resource
        /// </summary>
        /// <param name="resource">Resource name</param>
        /// <returns>List of audit logs for the resource</returns>
        /// <response code="200">Audit logs retrieved successfully</response>
        /// <response code="500">Server error</response>
        [HttpGet("resource/{resource}")]
        [ProducesResponseType(typeof(IEnumerable<GetAuditLogDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetByResource(string resource)
        {
            try
            {
                var dtos = await _service.GetByResourceAsync(resource);
                return Ok(new { message = "Audit logs retrieved successfully.", data = dtos });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving audit logs.", error = ex.Message });
            }
        }

        /// <summary>
        /// Get all audit logs
        /// </summary>
        /// <returns>List of all audit logs</returns>
        /// <response code="200">Audit logs retrieved successfully</response>
        /// <response code="500">Server error</response>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<GetAuditLogDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var dtos = await _service.GetAllAsync();
                return Ok(new { message = "Audit logs retrieved successfully.", data = dtos });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving audit logs.", error = ex.Message });
            }
        }

        /// <summary>
        /// Get audit logs by date range
        /// </summary>
        /// <param name="startDate">Start date</param>
        /// <param name="endDate">End date</param>
        /// <returns>List of audit logs within the date range</returns>
        /// <response code="200">Audit logs retrieved successfully</response>
        /// <response code="400">Invalid date range</response>
        /// <response code="500">Server error</response>
        [HttpGet("date-range")]
        [ProducesResponseType(typeof(IEnumerable<GetAuditLogDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetByDateRange(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate)
        {
            try
            {
                if (startDate > endDate)
                    return BadRequest(new { message = "Start date must be before or equal to end date." });

                var dtos = await _service.GetByDateRangeAsync(startDate, endDate);
                return Ok(new { message = "Audit logs retrieved successfully.", data = dtos });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving audit logs.", error = ex.Message });
            }
        }

        /// <summary>
        /// Delete (soft delete) an audit log
        /// </summary>
        /// <param name="auditId">Audit ID</param>
        /// <returns>Success or failure message</returns>
        /// <response code="200">Audit log deleted successfully</response>
        /// <response code="404">Audit log not found</response>
        /// <response code="500">Server error</response>
        [HttpDelete("{auditId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(int auditId)
        {
            try
            {
                var result = await _service.DeleteAsync(auditId);
                if (!result)
                    return NotFound(new { message = $"Audit log with ID {auditId} not found." });

                return Ok(new { message = "Audit log deleted successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while deleting the audit log.", error = ex.Message });
            }
        }
    }
}
