
//Hello??
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SwiftPay.Services.Interfaces;
using SwiftPay.DTOs.ComplianceDTO;
using SwiftPay.Domain.Remittance.Entities;

namespace SwiftPay.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ComplianceDecisionController : ControllerBase
    {
        private readonly IComplianceDecisionService _decisionService;

        public ComplianceDecisionController(IComplianceDecisionService decisionService)
        {
            _decisionService = decisionService;
        }

        // 1. CREATE (POST): api/ComplianceDecision
        // Records a new decision (Approve/Hold/Reject)
        [HttpPost]
        public async Task<ActionResult<ComplianceDecision>> Create([FromBody] CreateComplianceDecisionDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _decisionService.RecordDecisionAsync(dto);

            // Returns 201 Created and links to the GetById route
            return CreatedAtAction(nameof(GetById), new { id = result.DecisionId }, result);
        }

        // 2. READ ALL (GET): api/ComplianceDecision/remit/{remitId}
        // Gets the full history for a specific transaction
        [HttpGet("remit/{remitId}")]
        public async Task<ActionResult<IEnumerable<ComplianceDecision>>> GetByRemitId(string remitId)
        {
            var results = await _decisionService.GetDecisionsByRemittanceAsync(remitId);
            return Ok(results);
        }

        // 3. READ SINGLE (GET): api/ComplianceDecision/{id}
        // Gets one specific decision record
        [HttpGet("{id}")]
        public async Task<ActionResult<ComplianceDecision>> GetById(string id)
        {
            var result = await _decisionService.GetDecisionByIdAsync(id);
            if (result == null) return NotFound($"Decision with ID {id} not found.");

            return Ok(result);
        }

        // 4. UPDATE (PUT): api/ComplianceDecision/{id}
        // Updates the status or notes of an existing decision
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] UpdateComplianceDecisionDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var success = await _decisionService.UpdateDecisionAsync(id, dto);
            if (!success) return NotFound();

            return NoContent(); // 204 Success
        }

        // 5. DELETE (DELETE): api/ComplianceDecision/{id}
        // Performs a SOFT DELETE by flipping the IsDeleted flag
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var success = await _decisionService.SoftDeleteDecisionAsync(id);
            if (!success) return NotFound();

            return NoContent(); // 204 Success
        }
    }
}