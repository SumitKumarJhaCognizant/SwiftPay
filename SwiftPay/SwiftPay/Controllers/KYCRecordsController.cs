using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using SwiftPay.Services.Interfaces;
using SwiftPay.DTOs.UserCustomerDTO;

namespace SwiftPay.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class KYCRecordsController : ControllerBase
    {
        private readonly IKYCRecordService _service;

        public KYCRecordsController(IKYCRecordService service)
        {
            _service = service;
        }

        /// <summary>
        /// Create a new KYC record
        /// </summary>
        /// <param name="dto">KYC record creation data</param>
        /// <returns>Created KYC record DTO</returns>
        /// <response code="200">KYC record created successfully</response>
        /// <response code="400">Invalid request data</response>
        /// <response code="409">Business conflict (e.g., duplicate KYC record)</response>
        /// <response code="500">Server error</response>
        [HttpPost]
        [ProducesResponseType(typeof(KYCRecordResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Create([FromBody] CreateKYCRecordDto dto)
        {
            try
            {
                // Ensure regular users can only create KYC for themselves.
                // Admins and Compliance users may create records for any user.
                var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;
                if (!int.TryParse(idClaim, out var currentUserId))
                {
                    return Forbid();
                }

                var isPrivileged = User.IsInRole("Admin") || User.IsInRole("Compliance");

                // For regular users, force the UserID to the caller's user id (from JWT).
                if (!isPrivileged)
                {
                    dto.UserID = currentUserId;
                }
                else
                {
                    // Privileged callers (Admin/Compliance) may create for any user.
                    // If they omit UserID, default to their own id.
                    if (!dto.UserID.HasValue)
                        dto.UserID = currentUserId;
                }

                var created = await _service.CreateAsync(dto);
                return Ok(new { message = "KYC record created successfully.", data = created });
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
                return StatusCode(500, new { message = "An error occurred while creating the KYC record.", error = ex.Message });
            }
        }

        /// <summary>
        /// Get KYC record by ID
        /// </summary>
        /// <param name="kycId">KYC record ID</param>
        /// <returns>KYC record DTO</returns>
        /// <response code="200">KYC record found</response>
        /// <response code="404">KYC record not found</response>
        /// <response code="500">Server error</response>
        [HttpGet("{kycId}")]
        [Authorize(Roles = "Admin, Compliance")]
        [ProducesResponseType(typeof(KYCRecordResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetById(int kycId)
        {
            try
            {
                var kyc = await _service.GetByIdAsync(kycId);
                if (kyc == null)
                    return NotFound(new { message = $"KYC record with ID {kycId} not found." });

                return Ok(new { message = "KYC record retrieved successfully.", data = kyc });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving the KYC record.", error = ex.Message });
            }
        }

        /// <summary>
        /// Get KYC record by user ID
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>KYC record DTO</returns>
        /// <response code="200">KYC record found</response>
        /// <response code="404">KYC record not found</response>
        /// <response code="500">Server error</response>
        [HttpGet("user/{userId}")]
        // Allow owners to fetch their own KYC record; Admin and Compliance may fetch any.
        [Authorize]
        [ProducesResponseType(typeof(KYCRecordResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetByUserId(int userId)
        {
            try
            {
                // Authorization: owners or privileged roles only
                var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;
                if (!int.TryParse(idClaim, out var currentUserId))
                {
                    return Forbid();
                }

                var isPrivileged = User.IsInRole("Admin") || User.IsInRole("Compliance") || User.IsInRole("Agent");
                if (!isPrivileged && userId != currentUserId)
                {
                    return Forbid();
                }

                var kyc = await _service.GetByUserIdAsync(userId);
                if (kyc == null)
                    return NotFound(new { message = $"No KYC record found for user with ID {userId}." });

                return Ok(new { message = "KYC record retrieved successfully.", data = kyc });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving the KYC record.", error = ex.Message });
            }
        }

        /// <summary>
        /// Get all KYC records
        /// </summary>
        /// <returns>List of all KYC record DTOs</returns>
        /// <response code="200">KYC records retrieved successfully</response>
        /// <response code="500">Server error</response>
        [HttpGet]
        [Authorize(Roles = "Admin, Compliance")]
        [ProducesResponseType(typeof(IEnumerable<KYCRecordResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var kycRecords = await _service.GetAllAsync();
                return Ok(new { message = "KYC records retrieved successfully.", data = kycRecords });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving KYC records.", error = ex.Message });
            }
        }

        /// <summary>
        /// Get pending KYC records for compliance review (with pagination)
        /// </summary>
        /// <param name="pageNumber">Page number (default: 1)</param>
        /// <param name="pageSize">Page size (default: 10, max: 100)</param>
        /// <returns>List of pending KYC record DTOs with pagination metadata</returns>
        /// <response code="200">Pending KYC records retrieved successfully</response>
        /// <response code="400">Invalid pagination parameters</response>
        /// <response code="500">Server error</response>
        [HttpGet("pending")]
        [Authorize(Roles = "Admin, Compliance")]
        [ProducesResponseType(typeof(KYCRecordListDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetPending([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                // Validate custom business logic (pagination check)
                if (pageNumber < 1 || pageSize < 1 || pageSize > 100)
                    return BadRequest(new { message = "PageNumber must be >= 1 and PageSize must be between 1 and 100." });

                var response = await _service.GetPendingAsync(pageNumber, pageSize);
                return Ok(new { message = "Pending KYC records retrieved successfully.", data = response });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving pending KYC records.", error = ex.Message });
            }
        }

        /// <summary>
        /// Update KYC record details
        /// </summary>
        /// <param name="kycId">KYC record ID</param>
        /// <param name="dto">Update data</param>
        /// <returns>Updated KYC record DTO</returns>
        /// <response code="200">KYC record updated successfully</response>
        /// <response code="404">KYC record not found</response>
        /// <response code="409">Business conflict</response>
        /// <response code="500">Server error</response>
        [HttpPut("{kycId}")]
        [Authorize(Roles = "Admin, Compliance")]
        [ProducesResponseType(typeof(KYCRecordResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Update(int kycId, [FromBody] UpdateKYCRecordDto dto)
        {
            try
            {
                var updated = await _service.UpdateAsync(kycId, dto);
                return Ok(new { message = "KYC record updated successfully.", data = updated });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating the KYC record.", error = ex.Message });
            }
        }

        /// <summary>
        /// Update KYC verification status (Approve/Reject/Pending)
        /// </summary>
        /// <param name="kycId">KYC record ID</param>
        /// <param name="dto">Status update data with optional notes</param>
        /// <returns>Updated KYC record DTO with new status</returns>
        /// <response code="200">KYC status updated successfully</response>
        /// <response code="404">KYC record not found</response>
        /// <response code="500">Server error</response>
        [HttpPatch("{kycId}/status")]
        [Authorize(Roles = "Admin, Compliance")]
        [ProducesResponseType(typeof(KYCRecordResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateStatus(int kycId, [FromBody] UpdateKycStatusDto dto)
        {
            try
            {
                var updated = await _service.UpdateStatusAsync(kycId, dto);
                return Ok(new { message = "KYC record status updated successfully.", data = updated });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating the KYC status.", error = ex.Message });
            }
        }

        /// <summary>
        /// Mark KYC record as verified (Legacy endpoint - use UpdateStatus instead)
        /// </summary>
        /// <param name="kycId">KYC record ID</param>
        /// <returns>Updated KYC record DTO</returns>
        /// <response code="200">KYC record marked as verified</response>
        /// <response code="404">KYC record not found</response>
        /// <response code="500">Server error</response>
        [HttpPut("{kycId}/verify")]
        [Authorize(Roles = "Admin, Compliance")]
        [ProducesResponseType(typeof(KYCRecordResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> MarkAsVerified(int kycId)
        {
            try
            {
                var verified = await _service.MarkAsVerifiedAsync(kycId);
                return Ok(new { message = "KYC record marked as verified.", data = verified });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while verifying the KYC record.", error = ex.Message });
            }
        }

        /// <summary>
        /// Delete KYC record (soft delete)
        /// </summary>
        /// <param name="kycId">KYC record ID</param>
        /// <returns>Success or failure message</returns>
        /// <response code="200">KYC record deleted successfully</response>
        /// <response code="404">KYC record not found</response>
        /// <response code="500">Server error</response>
        [HttpDelete("{kycId}")]
        [Authorize(Roles = "Admin, Compliance")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(int kycId)
        {
            try
            {
                var deleted = await _service.DeleteAsync(kycId);
                if (!deleted)
                    return NotFound(new { message = $"KYC record with ID {kycId} not found." });

                return Ok(new { message = "KYC record deleted successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while deleting the KYC record.", error = ex.Message });
            }
        }
    }
}
