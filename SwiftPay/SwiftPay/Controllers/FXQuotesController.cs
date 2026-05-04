using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using SwiftPay.DTOs.FXQuoteDTO;
using SwiftPay.Services.Interfaces;

namespace SwiftPay.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // 1. BASE REQUIREMENT: Anyone accessing this controller must be logged in.
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
            if (request.SendAmount <= 0)
                return BadRequest(new { message = "Send amount must be greater than zero." });

            try
            {
                var response = await _service.GenerateQuoteAsync(request);
                return Ok(response);
            }
            catch (NotSupportedException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("{id}")]
        // 3. READ RULE: Both Customers and Admins can view the quote. (Note: No spaces in the string!)
        [Authorize(Roles = "Customer,Admin")] 
        public async Task<IActionResult> GetQuote(string id)
        {
            var response = await _service.GetQuoteAsync(id);
            if (response == null) return NotFound($"Quote with ID {id} not found.");
            
            return Ok(response);
        }
    }
}