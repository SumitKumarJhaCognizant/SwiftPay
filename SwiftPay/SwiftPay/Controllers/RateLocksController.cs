using Microsoft.AspNetCore.Authorization; 
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims; 
using System.Threading.Tasks;
using SwiftPay.DTOs.FXQuoteDTO;
using SwiftPay.Services.Interfaces;

namespace SwiftPay.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // 1. BASE LEVEL: Must be logged in.
    public class RateLocksController : ControllerBase
    {
        private readonly IRateLockService _service;

        public RateLocksController(IRateLockService service)
        {
            _service = service;
        }

        [HttpPost]
        [Authorize(Roles = "Customer,Agent,Admin")] // Customers self-serve; Agent/Admin act on behalf of customers.
        public async Task<IActionResult> CreateRateLock([FromBody] CreateRateLockRequestDto request)
        {
            var callerUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(callerUserId))
            {
                return Unauthorized("Invalid or missing user identity in token.");
            }

            // Customers can only lock for themselves; Agent/Admin may pass any CustomerID.
            if (User.IsInRole("Customer"))
            {
                request.CustomerID = callerUserId;
            }
            else if (string.IsNullOrWhiteSpace(request.CustomerID))
            {
                request.CustomerID = callerUserId;  // fallback if not provided
            }

            var response = await _service.LockRateAsync(request);
            return Ok(response);
        }
        
        [HttpGet("{id}")]
        [Authorize(Roles = "Customer,Admin,Treasury,Ops")]
        public async Task<IActionResult> GetRateLock(string id)
        {
            var response = await _service.GetRateLockAsync(id);
            if (response == null) return NotFound($"Rate Lock with ID {id} not found.");

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isPrivileged = User.IsInRole("Admin") || User.IsInRole("Treasury") || User.IsInRole("Ops");
            if (!isPrivileged && response.CustomerID != currentUserId)
                return Forbid();

            return Ok(response);
        }

        // Treasury / Admin can list all rate locks for oversight.
        [HttpGet]
        [Authorize(Roles = "Admin,Treasury,Ops")]
        public async Task<IActionResult> GetAll()
        {
            var all = await _service.GetAllRateLocksAsync();
            return Ok(new { message = "Rate locks retrieved.", data = all });
        }
    }
}