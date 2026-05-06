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
    public class CustomersController : ControllerBase
    {
        private readonly ICustomerService _service;
        private readonly IBeneficiaryService _beneficiaryService;
        private readonly IUserService _userService;

        public CustomersController(ICustomerService service, IBeneficiaryService beneficiaryService, IUserService userService)
        {
            _service = service;
            _beneficiaryService = beneficiaryService;
            _userService = userService;
        }

        /// <summary>
        /// Get paginated remittances for a customer
        /// </summary>
        [HttpGet("{customerId}/remittances")]
        [ProducesResponseType(typeof(IEnumerable<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetRemittances(int customerId, [FromQuery] string? status, [FromQuery] int page = 1, [FromQuery] int limit = 10)
        {
            try
            {
                // resolve remittance service from DI via controller's service provider? Use ICustomerService to fetch via remittance service not available here.
                // For simplicity, create a new scope and resolve IRemittanceService.
                var remittanceService = HttpContext.RequestServices.GetService(typeof(SwiftPay.Services.Interfaces.IRemittanceService)) as SwiftPay.Services.Interfaces.IRemittanceService;
                if (remittanceService == null) return StatusCode(500, new { message = "Remittance service not available." });

                var list = await remittanceService.GetByCustomerRemittancesAsync(customerId, page, limit, status);
                return Ok(new { message = "Remittances retrieved successfully.", data = list });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving remittances.", error = ex.Message });
            }
        }

        /// <summary>
        /// Create a new customer profile
        /// </summary>
        /// <param name="dto">Customer creation data</param>
        /// <returns>Created customer DTO</returns>
        /// <response code="200">Customer created successfully</response>
        /// <response code="400">Invalid request data</response>
        /// <response code="500">Server error</response>
        [HttpPost]
        [ProducesResponseType(typeof(CustomerResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Create([FromBody] CreateCustomerDto dto)
        {
            try
            {
                // Extract current user id from JWT (NameIdentifier preferred, fallback to sub)
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
                if (string.IsNullOrEmpty(userIdClaim))
                    return Forbid();

                if (!int.TryParse(userIdClaim, out var currentUserId))
                    return Forbid();

                // Role-based logic and validation:
                var isAdmin = User.IsInRole("Admin");
                var isOps = User.IsInRole("Ops");
                var isCustomer = User.IsInRole("Customer");

                if (isCustomer)
                {
                    // Customers can only create a profile for themselves; ignore any supplied UserID
                    dto.UserID = currentUserId;
                }
                else if (isAdmin || isOps)
                {
                    // Admin/Ops: if they supplied a UserID use it, otherwise default to their own id
                    if (!dto.UserID.HasValue)
                        dto.UserID = currentUserId;
                }
                else
                {
                    // Other roles should not be allowed to set arbitrary user ids; default to token id
                    dto.UserID = currentUserId;
                }

                // Final validation: ensure we have a UserID to associate the customer with
                if (!dto.UserID.HasValue)
                {
                    return BadRequest(new { message = "UserID is required." });
                }

                var created = await _service.CreateAsync(dto);
                return Ok(new { message = "Customer created successfully.", data = created });
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
                // Surface inner exception message when available (helps diagnose SQL constraint errors)
                var inner = ex.InnerException?.Message ?? ex.Message;
                return StatusCode(500, new { message = "An error occurred while creating the customer.", error = inner });
            }
        }

        /// <summary>
        /// Get customer by ID
        /// </summary>
        /// <param name="customerId">Customer ID</param>
        /// <returns>Customer DTO</returns>
        /// <response code="200">Customer found</response>
        /// <response code="404">Customer not found</response>
        /// <response code="500">Server error</response>
        [HttpGet("{customerId}")]
        [ProducesResponseType(typeof(CustomerResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetById(int customerId)
        {
            try
            {
                var customer = await _service.GetByIdAsync(customerId);
                if (customer == null)
                    return NotFound(new { message = $"Customer with ID {customerId} not found." });

                // Extract current user id from JWT (NameIdentifier preferred, fallback to sub)
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
                if (string.IsNullOrEmpty(userIdClaim))
                    return Forbid();

                if (!int.TryParse(userIdClaim, out var currentUserId))
                    return Forbid();

                // Privileged staff (Admin, Agent, Ops, Compliance) can read any customer profile.
                var isPrivileged = User.IsInRole("Admin") || User.IsInRole("Agent")
                                   || User.IsInRole("Ops") || User.IsInRole("Compliance");
                if (!isPrivileged && customer.UserID != currentUserId)
                    return Forbid();

                return Ok(new { message = "Customer retrieved successfully.", data = customer });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving the customer.", error = ex.Message });
            }
        }

        /// <summary>
        /// Get customer by User ID
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>Customer DTO</returns>
        /// <response code="200">Customer found</response>
        /// <response code="404">Customer not found</response>
        /// <response code="500">Server error</response>
        [HttpGet("user/{userId}")]
        [ProducesResponseType(typeof(CustomerResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetByUserId(int userId)
        {
            try
            {
                var customer = await _service.GetByUserIdAsync(userId);
                if (customer == null)
                    return NotFound(new { message = $"Customer with User ID {userId} not found." });

                return Ok(new { message = "Customer retrieved successfully.", data = customer });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving the customer.", error = ex.Message });
            }
        }

        /// <summary>
        /// Get all customers
        /// </summary>
        /// <returns>List of all active customer DTOs</returns>
        /// <response code="200">Customers retrieved successfully</response>
        /// <response code="500">Server error</response>
        [HttpGet]
        [Authorize(Roles = "Admin,Ops,Agent")]
        [ProducesResponseType(typeof(IEnumerable<CustomerResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var customers = await _service.GetAllAsync();
                return Ok(new { message = "Customers retrieved successfully.", data = customers });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving customers.", error = ex.Message });
            }
        }

        /// <summary>
        /// Update customer information
        /// </summary>
        /// <param name="customerId">Customer ID</param>
        /// <param name="dto">Customer update data</param>
        /// <returns>Updated customer DTO</returns>
        /// <response code="200">Customer updated successfully</response>
        /// <response code="404">Customer not found</response>
        /// <response code="400">Invalid request data</response>
        /// <response code="500">Server error</response>
        [HttpPut("{customerId}")]
        [ProducesResponseType(typeof(CustomerResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Update(int customerId, [FromBody] UpdateCustomerDto dto)
        {
            try
            {
                // Verify resource exists and perform ownership check
                var existing = await _service.GetByIdAsync(customerId);
                if (existing == null)
                    return NotFound(new { message = $"Customer with ID {customerId} not found." });

                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
                if (string.IsNullOrEmpty(userIdClaim))
                    return Forbid();

                if (!int.TryParse(userIdClaim, out var currentUserId))
                    return Forbid();

                if (!User.IsInRole("Admin"))
                {
                    if (existing.UserID != currentUserId)
                        return Forbid();
                }

                var updated = await _service.UpdateAsync(customerId, dto);
                return Ok(new { message = "Customer updated successfully.", data = updated });
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
                return StatusCode(500, new { message = "An error occurred while updating the customer.", error = ex.Message });
            }
        }

        /// <summary>
        /// Update customer risk rating
        /// </summary>
        /// <param name="customerId">Customer ID</param>
        /// <param name="dto">Risk rating update data</param>
        /// <returns>Updated customer DTO with new risk rating</returns>
        /// <response code="200">Customer risk rating updated successfully</response>
        /// <response code="404">Customer not found</response>
        /// <response code="500">Server error</response>
        [HttpPatch("{customerId}/risk-rating")]
        [Authorize(Roles = "Admin,Ops")]
        [ProducesResponseType(typeof(CustomerResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateRiskRating(int customerId, [FromBody] UpdateCustomerRiskRatingDto dto)
        {
            try
            {
                var updated = await _service.UpdateRiskRatingAsync(customerId, dto);
                return Ok(new { message = "Customer risk rating updated successfully.", data = updated });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating the customer risk rating.", error = ex.Message });
            }
        }

        /// <summary>
        /// Delete (soft delete) a customer
        /// </summary>
        /// <param name="customerId">Customer ID</param>
        /// <returns>Success or failure message</returns>
        /// <response code="200">Customer deleted successfully</response>
        /// <response code="404">Customer not found</response>
        /// <response code="500">Server error</response>
        [HttpDelete("{customerId}")]
        [Authorize(Roles = "Admin,Ops")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(int customerId)
        {
            try
            {
                var result = await _service.DeleteAsync(customerId);
                if (!result)
                    return NotFound(new { message = $"Customer with ID {customerId} not found." });

                return Ok(new { message = "Customer deleted successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while deleting the customer.", error = ex.Message });
            }
        }

        /// <summary>
        /// Delete the current customer's own profile (customers only). This endpoint ignores any client-supplied IDs and
        /// deletes the customer linked to the JWT subject. Beneficiaries are also deleted.
        /// </summary>
        [HttpDelete("me")]
        [Authorize(Roles = "Customer")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteMe()
        {
            try
            {
                // Extract current user id from JWT (NameIdentifier preferred, fallback to sub)
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
                if (string.IsNullOrEmpty(userIdClaim))
                    return Forbid();

                if (!int.TryParse(userIdClaim, out var currentUserId))
                    return Forbid();

                var linkedCustomer = await _service.GetByUserIdAsync(currentUserId);
                if (linkedCustomer == null)
                    return NotFound(new { message = "Customer profile for current user not found." });

                var targetCustomerId = linkedCustomer.CustomerID;
                var deleted = await _service.DeleteAsync(targetCustomerId);
                if (!deleted)
                    return NotFound(new { message = $"Customer with ID {targetCustomerId} not found." });

                // Cascade: delete all beneficiaries linked to this customer
                try
                {
                    var beneficiaries = await _beneficiaryService.GetByCustomerIdAsync(targetCustomerId);
                    if (beneficiaries != null)
                    {
                        foreach (var b in beneficiaries)
                        {
                            try { await _beneficiaryService.DeleteAsync(b.BeneficiaryID); } catch { }
                        }
                    }
                }
                catch { }

                // Also delete the linked user account for customer-initiated deletes
                try
                {
                    await _userService.DeleteAsync(currentUserId);
                }
                catch { }

                return Ok(new { message = "Customer and linked user deleted successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while deleting the customer.", error = ex.Message });
            }
        }
    }
}
