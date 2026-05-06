using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Security.Claims; // ADDED: Required to read the JWT Token claims
using SwiftPay.DTOs.FXQuoteDTO;
using SwiftPay.Services.Interfaces;

namespace SwiftPay.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] 
    public class FXQuotesController : ControllerBase
    {
        private readonly IFXQuoteService _service;

        public FXQuotesController(IFXQuoteService service)
        {
            _service = service;
        }

        [HttpPost]
        [Authorize(Roles = "Customer")] 
        public async Task<IActionResult> CreateQuote([FromBody] CreateQuoteRequestDto request)
        {
            // --- CATCH AND ATTACH THE ID ---
            // 1. Extract the secure ID from the user's logged-in token
            var customerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            // 2. Attach it to the DTO before sending it down to the service
            request.CustomerID = customerId;
            // -------------------------------

            var response = await _service.GenerateQuoteAsync(request);
            return Ok(response); 
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Customer,Admin")] 
        public async Task<IActionResult> GetQuote(string id)
        {
            var response = await _service.GetQuoteAsync(id);
            if (response == null) return NotFound($"Quote with ID {id} not found.");
            return Ok(response);
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Treasury,Ops")]
        public async Task<IActionResult> GetAll()
        {
            var all = await _service.GetAllQuotesAsync();
            return Ok(new { message = "FX quotes retrieved.", data = all });
        }
    }
}