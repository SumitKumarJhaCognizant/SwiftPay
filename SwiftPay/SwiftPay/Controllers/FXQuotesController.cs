using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using SwiftPay.DTOs.FXQuoteDTO;
using SwiftPay.Services.Interfaces;

namespace SwiftPay.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FXQuotesController : ControllerBase
    {
        private readonly IFXQuoteService _service;

        public FXQuotesController(IFXQuoteService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<IActionResult> CreateQuote([FromBody] CreateQuoteRequestDto request)
        {
            var response = await _service.GenerateQuoteAsync(request);
            return Ok(response); // Returns a 200 OK status with your JSON payload
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> GetQuote(string id)
        {
            var response = await _service.GetQuoteAsync(id);
            if (response == null) return NotFound($"Quote with ID {id} not found.");
            
            return Ok(response);
        }
    }
}