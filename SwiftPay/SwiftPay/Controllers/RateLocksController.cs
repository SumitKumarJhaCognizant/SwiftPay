using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using SwiftPay.DTOs.FXQuoteDTO;
using SwiftPay.Services.Interfaces;

namespace SwiftPay.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RateLocksController : ControllerBase
    {
        private readonly IRateLockService _service;

        public RateLocksController(IRateLockService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<IActionResult> CreateRateLock([FromBody] CreateRateLockRequestDto request)
        {
            var response = await _service.LockRateAsync(request);
            return Ok(response);
        }

        
        [HttpGet("{id}")]
        public async Task<IActionResult> GetRateLock(string id)
        {
            var response = await _service.GetRateLockAsync(id);
            if (response == null) return NotFound($"Rate Lock with ID {id} not found.");
            
            return Ok(response);
        }
    }
}