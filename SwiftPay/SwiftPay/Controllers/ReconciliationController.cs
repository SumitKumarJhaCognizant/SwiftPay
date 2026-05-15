using Microsoft.AspNetCore.Authorization; 
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SwiftPay.Services.Interfaces;
using SwiftPay.DTOs.ReconciliationDTO;
using SwiftPay.Models;
using SwiftPay.Constants.Enums;

namespace SwiftPay.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	
	[Authorize]
	public class ReconciliationController : ControllerBase
	{
		private readonly IReconciliationService _service;

		public ReconciliationController(IReconciliationService service)
		{
			_service = service;
		}

		// --------------------------------------------------------
		// CORE BUSINESS LOGIC ENDPOINTS 
		// --------------------------------------------------------

       /// <summary>
		/// Trigger reconciliation for a settlement batch identified by <paramref name="batchId"/>.
		/// Returns the list of generated reconciliation records for the batch.
		/// Requires roles: Treasury or Admin.
		/// </summary>
		/// <param name="batchId">Identifier of the settlement batch to reconcile.</param>
		/// <returns>HTTP 200 with reconciliation records on success; HTTP 404 or 400 on errors.</returns>
		[HttpPost("batch/{batchId}")]
		[Authorize(Roles = "Treasury, Admin,Ops")] 
		public async Task<ActionResult<IEnumerable<ReconciliationRecord>>> ReconcileBatch(int batchId)
		{
			try
			{
				var records = await _service.ReconcileBatchAsync(batchId);
				return Ok(records);
			}
			catch (KeyNotFoundException ex)
			{
				return NotFound(new { Message = ex.Message });
			}
			catch (InvalidOperationException ex)
			{
				return BadRequest(new { Message = ex.Message });
			}
		}


		


     /// <summary>
		/// Auto-reconcile a single reference (Remit or Instruction) identified by <paramref name="referenceId"/>.
		/// Produces a single reconciliation record for the provided reference.
		/// Requires roles: Treasury or Admin.
		/// </summary>
		/// <param name="type">The reference type (Remit or Instruction).</param>
		/// <param name="referenceId">The identifier of the reference to reconcile.</param>
		/// <returns>HTTP 200 with the created reconciliation record; HTTP 404 or 400 on errors.</returns>
		[HttpPost("auto/{type}/{referenceId}")]
		[Authorize(Roles = "Treasury, Admin, Ops")] 
		public async Task<ActionResult<ReconciliationRecord>> AutoReconcile(ReferenceType type, string referenceId)
		{
			try
			{
				var record = await _service.AutoReconcileAsync(type, referenceId);
				return Ok(record);
			}
			catch (KeyNotFoundException ex)
			{
				return NotFound(new { Message = ex.Message });
			}
			catch (NotSupportedException ex)
			{
				return BadRequest(new { Message = ex.Message });
			}
		}

		// --------------------------------------------------------
		// STANDARD CRUD ENDPOINTS (Read-Only access for Watchers)
		// --------------------------------------------------------

       /// <summary>
		/// Retrieve all reconciliation records.
		/// Requires roles: Treasury, Admin, Compliance, or Ops.
		/// </summary>
		/// <returns>HTTP 200 with a list of reconciliation records.</returns>
		[HttpGet]
		[Authorize(Roles = "Treasury, Admin, Compliance, Ops")] 
		public async Task<ActionResult<IEnumerable<ReconciliationRecord>>> GetAll()
		{
			var records = await _service.GetAllAsync();
			return Ok(records);
		}

       /// <summary>
		/// Retrieve a single reconciliation record by its numeric identifier.
		/// Requires roles: Treasury, Admin, Compliance, or Ops.
		/// </summary>
		/// <param name="id">Reconciliation record identifier.</param>
		/// <returns>HTTP 200 with the record if found; HTTP 404 if not found.</returns>
		[HttpGet("{id}")]
		[Authorize(Roles = "Treasury, Admin, Compliance, Ops")]
		public async Task<ActionResult<ReconciliationRecord>> GetById(int id)
		{
			try
			{
				var record = await _service.GetByIdAsync(id);
				return Ok(record);
			}
			catch (KeyNotFoundException)
			{
				return NotFound();
			}
		}

     /// <summary>
		/// Retrieve reconciliation records with mismatched results.
		/// Requires roles: Treasury, Admin, Compliance, or Ops.
		/// </summary>
		/// <returns>HTTP 200 with a list of mismatched reconciliation records.</returns>
		[HttpGet("mismatches")]
		[Authorize(Roles = "Treasury, Admin, Compliance, Ops")] 
		public async Task<ActionResult<IEnumerable<ReconciliationRecord>>> GetMismatches()
		{
			var records = await _service.GetMismatchesAsync();
			return Ok(records);
		}

        /// <summary>
		/// Retrieve reconciliation records filtered by reference type.
		/// Requires roles: Treasury, Admin, Compliance, or Ops.
		/// </summary>
		/// <param name="type">Reference type to filter by (Remit or Instruction).</param>
		/// <returns>HTTP 200 with a list of matching reconciliation records.</returns>
		[HttpGet("type/{type}")]
		[Authorize(Roles = "Treasury, Admin, Compliance, Ops")]
		public async Task<ActionResult<IEnumerable<ReconciliationRecord>>> GetByType(ReferenceType type)
		{
			var records = await _service.GetByTypeAsync(type);
			return Ok(records);
		}

		// --------------------------------------------------------
		// MANUAL OVERRIDES & DELETIONS (Strict Privilege)
		// --------------------------------------------------------




      /// <summary>
		/// Create a reconciliation record manually. Intended for manual overrides.
		/// Requires roles: Treasury or Admin.
		/// </summary>
		/// <param name="dto">Payload describing the reconciliation record to create.</param>
		/// <returns>HTTP 201 with the created reconciliation record.</returns>
		[HttpPost]
		[Authorize(Roles = "Treasury, Admin,Ops")] 
		public async Task<ActionResult<ReconciliationRecord>> Create([FromBody] CreateReconciliationDto dto)
		{
			try
			{
				var record = await _service.CreateAsync(dto);
				return CreatedAtAction(nameof(GetById), new { id = record.ReconID }, record);
			}
			catch (ArgumentException ex)
			{
				return BadRequest(new { Message = ex.Message });
			}
		}

       /// <summary>
		/// Update an existing reconciliation record.
		/// Requires roles: Treasury, Admin, or Compliance.
		/// </summary>
		/// <param name="id">Identifier of the reconciliation record to update.</param>
		/// <param name="dto">Update payload containing new values.</param>
		/// <returns>HTTP 200 with the updated reconciliation record; HTTP 404 if not found.</returns>
		[HttpPut("{id}")]
		[Authorize(Roles = "Treasury, Admin, Compliance,Ops")] 
		public async Task<ActionResult<ReconciliationRecord>> Update(int id, [FromBody] UpdateReconciliationDto dto)
		{
			if (id != dto.ReconID) return BadRequest("ID mismatch between URL and payload.");

			try
			{
				var record = await _service.UpdateAsync(dto);
				return Ok(record);
			}
			catch (KeyNotFoundException ex)
			{
				return NotFound(new { Message = ex.Message });
			}
		}

        /// <summary>
		/// Soft-delete a reconciliation record by id. Only users in the Admin role may delete.
		/// </summary>
		/// <param name="id">The reconciliation record identifier.</param>
		/// <returns>HTTP 204 on success; HTTP 404 if the record does not exist.</returns>
		[HttpDelete("{id}")]
		[Authorize(Roles = "Admin,Ops")] 
		public async Task<IActionResult> DeleteReconciliationRecord([FromRoute] int id)
		{
			var success = await _service.DeleteAsync(id);
			if (!success) return NotFound($"Record {id} not found.");

			return NoContent();
		}
	}
}