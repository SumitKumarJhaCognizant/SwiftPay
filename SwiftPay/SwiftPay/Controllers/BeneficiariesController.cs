using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Collections.Generic;
using SwiftPay.Services.Interfaces;
using SwiftPay.DTOs.UserCustomerDTO;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

namespace SwiftPay.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class BeneficiariesController : ControllerBase
    {
        private readonly IBeneficiaryService _service;
        private readonly ICustomerService _customerService;

        public BeneficiariesController(IBeneficiaryService service, ICustomerService customerService)
        {
            _service = service;
            _customerService = customerService;
        }

        /// <summary>
        /// Create a new beneficiary
        /// </summary>
        /// <param name="dto">Beneficiary creation data</param>
        /// <returns>Created beneficiary DTO</returns>
        /// <response code="200">Beneficiary created successfully</response>
        /// <response code="400">Invalid request data</response>
        /// <response code="500">Server error</response>
        [HttpPost]
        [ProducesResponseType(typeof(BeneficiaryResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Create([FromBody] CreateBeneficiaryDto dto)
        {
            try
            {
                // Extract current user id from token
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
                if (string.IsNullOrEmpty(userIdClaim)) return Forbid();
                if (!int.TryParse(userIdClaim, out var currentUserId)) return Forbid();

                // Look up the customer's profile linked to this user (if any)
                var linkedCustomer = await _customerService.GetByUserIdAsync(currentUserId);

                // Role-based behavior:
                // - Customers: force CustomerID to their linked profile (do not trust JSON)
                // - Admin/Ops: allow providing CustomerID; if omitted, default to their linked customer
                if (User.IsInRole("Customer"))
                {
                    if (linkedCustomer == null)
                        return BadRequest(new { message = "Please complete your profile first." });

                    dto.CustomerID = linkedCustomer.CustomerID;
                }
                else if (User.IsInRole("Admin") || User.IsInRole("Ops"))
                {
                    if (!dto.CustomerID.HasValue)
                    {
                        if (linkedCustomer != null)
                            dto.CustomerID = linkedCustomer.CustomerID;
                        else
                            return BadRequest(new { message = "CustomerID not provided and no linked customer found for your account." });
                    }
                }

                var created = await _service.CreateAsync(dto);
                return Ok(new { message = "Beneficiary created successfully.", data = created });
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
                // Surface inner exception message to help diagnose SQL constraint errors (kept identical to customer flow)
                return StatusCode(500, new { message = ex.InnerException?.Message ?? ex.Message });
            }
        }

        /// <summary>
        /// Get beneficiary by ID
        /// </summary>
        /// <param name="beneficiaryId">Beneficiary ID</param>
        /// <returns>Beneficiary DTO</returns>
        /// <response code="200">Beneficiary found</response>
        /// <response code="404">Beneficiary not found</response>
        /// <response code="500">Server error</response>
        [HttpGet("{beneficiaryId}")]
        [ProducesResponseType(typeof(BeneficiaryResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetById(int beneficiaryId)
        {
            try
            {
                var beneficiary = await _service.GetByIdAsync(beneficiaryId);
                if (beneficiary == null)
                    return NotFound(new { message = $"Beneficiary with ID {beneficiaryId} not found." });

                // Ownership check: only the owner customer or Admin/Ops can access
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
                if (string.IsNullOrEmpty(userIdClaim))
                    return Forbid();

                if (!int.TryParse(userIdClaim, out var currentUserId))
                    return Forbid();

                // Privileged staff (Admin, Agent, Ops, Compliance) can read any beneficiary.
                var isPrivileged = User.IsInRole("Admin") || User.IsInRole("Agent")
                                   || User.IsInRole("Ops") || User.IsInRole("Compliance");
                if (!isPrivileged)
                {
                    var customer = await _customerService.GetByIdAsync(beneficiary.CustomerID);
                    if (customer == null || customer.UserID != currentUserId) return Forbid();
                }

                return Ok(new { message = "Beneficiary retrieved successfully.", data = beneficiary });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving the beneficiary.", error = ex.Message });
            }
        }

        /// <summary>
        /// Get all beneficiaries for a customer
        /// </summary>
        /// <param name="customerId">Customer ID</param>
        /// <returns>List of beneficiary DTOs for the customer</returns>
        /// <response code="200">Beneficiaries retrieved successfully</response>
        /// <response code="500">Server error</response>
        [HttpGet("customer/{customerId}")]
        [ProducesResponseType(typeof(IEnumerable<BeneficiaryResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetByCustomerId(int customerId)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
                if (string.IsNullOrEmpty(userIdClaim))
                    return Forbid();

                if (!int.TryParse(userIdClaim, out var currentUserId))
                    return Forbid();

                if (!User.IsInRole("Admin") && !User.IsInRole("Ops") && !User.IsInRole("Agent"))
                {
                    var customer = await _customerService.GetByIdAsync(customerId);
                    if (customer == null) return NotFound(new { message = $"Customer with ID {customerId} not found." });
                    if (customer.UserID != currentUserId) return Forbid();
                }

                var beneficiaries = await _service.GetByCustomerIdAsync(customerId);
                return Ok(new { message = "Beneficiaries retrieved successfully.", data = beneficiaries });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving beneficiaries.", error = ex.Message });
            }
        }

        /// <summary>
        /// Get all beneficiaries
        /// </summary>
        /// <returns>List of all active beneficiary DTOs</returns>
        /// <response code="200">Beneficiaries retrieved successfully</response>
        /// <response code="500">Server error</response>
        [HttpGet]
        [Authorize(Roles = "Admin,Ops")]
        [ProducesResponseType(typeof(IEnumerable<BeneficiaryResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var beneficiaries = await _service.GetAllAsync();
                return Ok(new { message = "Beneficiaries retrieved successfully.", data = beneficiaries });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving beneficiaries.", error = ex.Message });
            }
        }

        /// <summary>
        /// Update beneficiary information
        /// </summary>
        /// <param name="beneficiaryId">Beneficiary ID</param>
        /// <param name="dto">Beneficiary update data</param>
        /// <returns>Updated beneficiary DTO</returns>
        /// <response code="200">Beneficiary updated successfully</response>
        /// <response code="404">Beneficiary not found</response>
        /// <response code="400">Invalid request data</response>
        /// <response code="500">Server error</response>
        [HttpPut("{beneficiaryId}")]
        [ProducesResponseType(typeof(BeneficiaryResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Update(int beneficiaryId, [FromBody] UpdateBeneficiaryDto dto)
        {
            try
            {
                var existing = await _service.GetByIdAsync(beneficiaryId);
                if (existing == null)
                    return NotFound(new { message = $"Beneficiary with ID {beneficiaryId} not found." });

                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
                if (string.IsNullOrEmpty(userIdClaim)) return Forbid();
                if (!int.TryParse(userIdClaim, out var currentUserId)) return Forbid();

                if (!User.IsInRole("Admin") && !User.IsInRole("Ops"))
                {
                    var customer = await _customerService.GetByIdAsync(existing.CustomerID);
                    if (customer == null) return Forbid();
                    if (customer.UserID != currentUserId) return Forbid();
                }

                var updated = await _service.UpdateAsync(beneficiaryId, dto);
                return Ok(new { message = "Beneficiary updated successfully.", data = updated });
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
                return StatusCode(500, new { message = "An error occurred while updating the beneficiary.", error = ex.Message });
            }
        }

        /// <summary>
        /// Update beneficiary verification status
        /// </summary>
        /// <param name="beneficiaryId">Beneficiary ID</param>
        /// <param name="dto">Verification status update data</param>
        /// <returns>Updated beneficiary DTO with new verification status</returns>
        /// <response code="200">Beneficiary verification status updated successfully</response>
        /// <response code="404">Beneficiary not found</response>
        /// <response code="500">Server error</response>
        [HttpPatch("{beneficiaryId}/verification-status")]
        [Authorize(Roles = "Admin,Ops")]
        [ProducesResponseType(typeof(BeneficiaryResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateVerificationStatus(int beneficiaryId, [FromBody] UpdateBeneficiaryVerificationStatusDto dto)
        {
            try
            {
                var updated = await _service.UpdateVerificationStatusAsync(beneficiaryId, dto);
                return Ok(new { message = "Beneficiary verification status updated successfully.", data = updated });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating the beneficiary verification status.", error = ex.Message });
            }
        }

        /// <summary>
        /// Delete (soft delete) a beneficiary
        /// </summary>
        /// <param name="beneficiaryId">Beneficiary ID</param>
        /// <returns>Success or failure message</returns>
        /// <response code="200">Beneficiary deleted successfully</response>
        /// <response code="404">Beneficiary not found</response>
        /// <response code="500">Server error</response>
        [HttpDelete("{beneficiaryId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(int beneficiaryId)
        {
            try
            {
                var existing = await _service.GetByIdAsync(beneficiaryId);
                if (existing == null)
                    return NotFound(new { message = $"Beneficiary with ID {beneficiaryId} not found." });

                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
                if (string.IsNullOrEmpty(userIdClaim)) return Forbid();
                if (!int.TryParse(userIdClaim, out var currentUserId)) return Forbid();

                if (!User.IsInRole("Admin") && !User.IsInRole("Ops"))
                {
                    var customer = await _customerService.GetByIdAsync(existing.CustomerID);
                    if (customer == null) return Forbid();
                    if (customer.UserID != currentUserId) return Forbid();
                }

                var result = await _service.DeleteAsync(beneficiaryId);
                if (!result)
                    return NotFound(new { message = $"Beneficiary with ID {beneficiaryId} not found." });

                return Ok(new { message = "Beneficiary deleted successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while deleting the beneficiary.", error = ex.Message });
            }
        }
    }
}
