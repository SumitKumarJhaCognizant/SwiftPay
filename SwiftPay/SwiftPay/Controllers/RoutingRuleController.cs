using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SwiftPay.DTOs.RoutingDTO;
using SwiftPay.Services.Interfaces;

namespace SwiftPay.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Treasury,Admin")]
    public class RoutingRuleController : ControllerBase
    {
        private readonly IRoutingRuleService _service;

        public RoutingRuleController(IRoutingRuleService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateRoutingRuleDto dto)
        {
            var result = await _service.CreateRuleAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = result.RuleId }, result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var result = await _service.GetRuleByIdAsync(id);
            return result != null ? Ok(result) : NotFound();
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _service.GetAllRulesAsync());
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] CreateRoutingRuleDto dto)
        {
            var success = await _service.UpdateRuleAsync(id, dto);
            return success ? NoContent() : NotFound();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var success = await _service.DeleteRuleAsync(id);
            return success ? NoContent() : NotFound();
        }
    }
}