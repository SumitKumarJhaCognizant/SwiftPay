using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using SwiftPay.Services.Interfaces;
using SwiftPay.DTOs.ComplianceDTO; // Ensure this matches your DTO namespace

namespace SwiftPay.Controllers
{
    [Route("api/[controller]")] // This makes your URL: api/Compliance
    [ApiController]             // This tells ASP.NET this is a Web API
    [Authorize(Roles = "Compliance,Admin")]
    public class ComplianceController : ControllerBase // ": ControllerBase" is essential
    {
        private readonly IComplianceCheckService _service;

        // Constructor: This is where you "inject" your service
        public ComplianceController(IComplianceCheckService service)
        {
            _service = service;
        }

        [HttpPost] // This allows the "Create" action via an HTTP POST request
        public async Task<IActionResult> Create([FromBody] CreateComplianceCheckDto dto)
        {
            try
            {
                var created = await _service.CreateAsync(dto);
                return Ok(created);
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
                return StatusCode(500, new { message = "An error occurred while creating compliance check.", error = ex.Message });
            }
        }
    }
}