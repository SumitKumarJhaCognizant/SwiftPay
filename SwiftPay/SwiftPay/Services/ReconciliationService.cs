using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SwiftPay.Services.Interfaces;
using SwiftPay.Repositories.Interfaces;
using SwiftPay.DTOs.ReconciliationDTO;
using SwiftPay.Models; // Contains RemittanceRequest, SettlementBatch, ReconciliationRecord
using SwiftPay.Domain.Remittance.Entities; // Contains PayoutInstruction
using SwiftPay.Constants.Enums;
using SwiftPay.Configuration;
using AutoMapper;

namespace SwiftPay.Services
{
	public class ReconciliationService : IReconciliationService
	{
		private readonly IReconciliationRepository _repo;
		private readonly AppDbContext _context;
		private readonly IMapper _mapper;
		private readonly ISettlementBatchRepository _settleRepo;

		public ReconciliationService(IReconciliationRepository repo, AppDbContext context, IMapper mapper, ISettlementBatchRepository settleRepo)
		{
			_repo = repo;
			_context = context;
			_mapper = mapper;
			_settleRepo = settleRepo; // Ensure this is assigned in the constructor
		}

		public async Task<IEnumerable<ReconciliationRecord>> ReconcileBatchAsync(int batchId)
		{
			// UPDATED: Using _settleRepo instead of _context
			var batch = await _settleRepo.GetByIdAsync(batchId)
				?? throw new KeyNotFoundException($"Settlement Batch {batchId} not found.");

			if (batch.Status == Status.Reconciled)
			{
				throw new InvalidOperationException($"Batch {batchId} is already successfully Reconciled and locked.");
			}

			var currencies = batch.Corridor.Split('-');
			string fromCurr = currencies[0].Trim();
			string toCurr = currencies[1].Trim();

			var allInstructions = await _context.PayoutInstructions.ToListAsync();

			var settledInstructions = allInstructions
				.Where(p => p.PartnerStatus == PayOutInstructionStatus.Settled
						 && p.SentDate >= batch.PeriodStart
						 && p.SentDate <= batch.PeriodEnd)
				.ToList();

			var remitIds = settledInstructions.Select(i => i.RemitId.Trim()).ToList();

			var matchingRemits = await _context.RemittanceRequests
				.Where(r => remitIds.Contains((r.RemitId).ToString().Trim())
						 && r.FromCurrency.Trim() == fromCurr
						 && r.ToCurrency.Trim() == toCurr)
				.ToListAsync();

			var joinedData = settledInstructions
				.Join(matchingRemits,
					  inst => inst.RemitId.Trim(),
					  remit => remit.RemitId.ToString().Trim(),
					  (inst, remit) => new { Instruction = inst, Remit = remit })
				.ToList();

			//  Stop if we found nothing!
			if (!joinedData.Any())
			{
				throw new Exception($"CRITICAL: We found 0 matching transactions for Batch {batchId} in the database. Halting reconciliation to prevent false success.");
			}

			
			// THE RECONCILIATION LOOP
			
			var records = new List<ReconciliationRecord>();
			const decimal tolerance = 0.01m;
			bool hasDiscrepancy = false;

			foreach (var data in joinedData)
			{
				decimal expected = data.Remit.ReceiverAmount ?? 0;
				decimal actual = ExtractPartnerAmountFromJson(data.Instruction.PayloadJson);

				bool isMatch = Math.Abs(expected - actual) <= tolerance;

				if (!isMatch) hasDiscrepancy = true; // Alarm triggered!

				var record = new ReconciliationRecord
				{
					ReferenceType = ReferenceType.Instruction,
					ReferenceID = data.Instruction.InstructionId,
					ExpectedAmount = expected,
					ActualAmount = actual,
					ReconDate = DateTime.UtcNow,
					CreatedDate = DateTime.UtcNow,
					UpdateDate = DateTime.UtcNow,
					Result = isMatch ? Result.Matched : Result.Mismatched,
					Notes = isMatch
						? $"Auto-matched via Batch {batchId}"
						: $"MISMATCH FOUND! Expected {expected}, Partner paid {actual}."
				};

				var savedRecord = await _repo.CreateAsync(record);
				records.Add(savedRecord);
			}

		
			//  STATUS GATEKEEPER & TIMESTAMP FIX
			
			if (hasDiscrepancy)
			{
				batch.Status = Status.Open; // Hold it open!
			}
			else
			{
				batch.Status = Status.Reconciled;
			}

			
			batch.UpdateDate = DateTime.UtcNow;

			await _settleRepo.UpdateAsync(batch);

			return records;
		}

		/// <summary>
		/// Attempts to automatically reconcile a remittance or payout instruction by comparing the expected and actual
		/// amounts based on the provided reference type and identifier.
		/// </summary>
		/// <param name="type">The type of reference to use for reconciliation. Must be either ReferenceType.Remit or ReferenceType.Instruction.</param>
		/// <param name="referenceId">The unique identifier of the remittance or payout instruction to reconcile, depending on the specified reference
		/// type.</param>
		/// <returns>A task that represents the asynchronous operation. The task result contains a ReconciliationRecord with the
		/// details of the reconciliation.</returns>
		/// <exception cref="KeyNotFoundException">Thrown if the specified remittance or payout instruction, or a required linked record, cannot be found for the
		/// given reference identifier.</exception>
		/// <exception cref="NotSupportedException">Thrown if the provided reference type is not supported for reconciliation.</exception>
	
		public async Task<ReconciliationRecord> AutoReconcileAsync(ReferenceType type, string referenceId)
		{
			RemittanceRequest remit = null;
			PayoutInstruction inst = null;

			// Fetch the linked records based on the ID provided.
			// RemittanceRequest.RemitId is an int — coerce the string referenceId.
			if (type == ReferenceType.Remit)
			{
				if (!int.TryParse(referenceId, out var remitIntId))
					throw new KeyNotFoundException($"Invalid remittance id '{referenceId}'.");

				remit = await _context.RemittanceRequests.FindAsync(remitIntId)
					?? throw new KeyNotFoundException($"Remittance {referenceId} not found.");

				inst = await _context.PayoutInstructions.FirstOrDefaultAsync(p => p.RemitId == referenceId);
			}
			else if (type == ReferenceType.Instruction)
			{
				inst = await _context.PayoutInstructions.FindAsync(referenceId)
					?? throw new KeyNotFoundException($"Instruction {referenceId} not found.");

				if (!int.TryParse(inst.RemitId, out var linkedRemitId))
					throw new KeyNotFoundException($"Instruction {referenceId} has invalid remit reference.");

				remit = await _context.RemittanceRequests.FindAsync(linkedRemitId)
					?? throw new KeyNotFoundException($"Linked remittance not found.");
			}
			else
			{
				throw new NotSupportedException($"ReferenceType {type} is not supported.");
			}

			// Apply correct comparison logic (Internal Destination Amount vs External Partner Payload)
			decimal expected = remit?.ReceiverAmount ?? 0;
			decimal actual = inst != null ? ExtractPartnerAmountFromJson(inst.PayloadJson) : 0;

			var dto = new CreateReconciliationDto
			{
				ReferenceType = type,
				ReferenceID = referenceId,
				ExpectedAmount = expected,
				ActualAmount = actual,

			};

			return await CreateAsync(dto);
		}

		/// <summary>
		/// Extracts the value of the "PayoutAmount" property from a JSON payload.
		/// </summary>
		/// <remarks>If the payload is null, empty, or does not contain a valid "PayoutAmount" property, the method
		/// returns 0. Invalid or malformed JSON is also handled by returning 0.</remarks>
		/// <param name="payloadJson">A JSON-formatted string containing a "PayoutAmount" property. Can be null or empty.</param>
		/// <returns>The decimal value of the "PayoutAmount" property if present and valid; otherwise, 0.</returns>
		private decimal ExtractPartnerAmountFromJson(string payloadJson)
		{
			if (string.IsNullOrWhiteSpace(payloadJson)) return 0;

			try
			{
				using JsonDocument doc = JsonDocument.Parse(payloadJson);
				if (doc.RootElement.TryGetProperty("PayoutAmount", out JsonElement amountElement))
				{
					if (amountElement.TryGetDecimal(out decimal amount))
					{
						return amount;
					}
				}
				return 0;
			}
			catch (JsonException)
			{
				return 0;
			}
		}

	  /// <summary>
	  /// Asynchronously creates a new reconciliation record using the specified data transfer object.
	  /// </summary>
	  /// <param name="dto">The data transfer object containing the information required to create a reconciliation record. Cannot be null.</param>
	  /// <returns>A task that represents the asynchronous operation. The task result contains the created reconciliation record.</returns>
		public async Task<ReconciliationRecord> CreateAsync(CreateReconciliationDto dto)
		{
			var entity = _mapper.Map<ReconciliationRecord>(dto);

			

			return await _repo.CreateAsync(entity);
		}

		/// <summary>
		/// Updates an existing reconciliation record with the specified values.
		/// </summary>
		/// <param name="dto">An object containing the reconciliation record identifier and the updated values to apply. The identifier must
		/// correspond to an existing record.</param>
		/// <returns>A task that represents the asynchronous operation. The task result contains the updated reconciliation record.</returns>
		/// <exception cref="KeyNotFoundException">Thrown if a reconciliation record with the specified identifier does not exist.</exception>

		public async Task<ReconciliationRecord> UpdateAsync(UpdateReconciliationDto dto)
		{
			var record = await _repo.GetByIdAsync(dto.ReconID)
				?? throw new KeyNotFoundException($"Reconciliation record {dto.ReconID} not found.");

			record.Result = dto.Result;
			record.Notes = dto.Notes;

			return await _repo.UpdateAsync(record);
		}

		public async Task<ReconciliationRecord> GetByIdAsync(int id)
		{
			return await _repo.GetByIdAsync(id)
				?? throw new KeyNotFoundException($"Reconciliation record {id} not found.");
		}
		/// <summary>
		/// Asynchronously retrieves all reconciliation records.
		/// </summary>
		/// <returns>A task that represents the asynchronous operation. The task result contains an enumerable collection of all
		/// reconciliation records.</returns>
		public async Task<IEnumerable<ReconciliationRecord>> GetAllAsync()
		{
			return await _repo.GetAllAsync();
		}

		/// <summary>
		/// Asynchronously retrieves all reconciliation records that are identified as mismatches.
		/// </summary>
		/// <returns>A task that represents the asynchronous operation. The task result contains a collection of <see
		/// cref="ReconciliationRecord"/> objects representing mismatched records. The collection is empty if no mismatches
		/// are found.</returns>
		public async Task<IEnumerable<ReconciliationRecord>> GetMismatchesAsync()
		{
			return await _repo.GetByResultAsync(Result.Mismatched);
		}
		/// <summary>
		/// Asynchronously retrieves all reconciliation records that match the specified reference type.
		/// </summary>
		/// <param name="type">The reference type used to filter reconciliation records.</param>
		/// <returns>A task that represents the asynchronous operation. The task result contains a collection of reconciliation records
		/// matching the specified reference type. The collection will be empty if no records are found.</returns>

		public async Task<IEnumerable<ReconciliationRecord>> GetByTypeAsync(ReferenceType type)
		{
			return await _repo.GetByReferenceTypeAsync(type);
		}


		/// <summary>
		/// Asynchronously marks the record with the specified identifier as deleted using a soft delete operation.
		/// </summary>
		/// <remarks>This method performs a soft delete by setting a deletion flag on the record rather than removing
		/// it from the data store. If the record does not exist, the method returns <see langword="false"/> and no action is
		/// taken.</remarks>
		/// <param name="id">The unique identifier of the record to delete.</param>
		/// <returns>A task that represents the asynchronous operation. The task result is <see langword="true"/> if the record was
		/// found and marked as deleted; otherwise, <see langword="false"/>.</returns>
		public async Task<bool> DeleteAsync(int id)
		{
			// 1. Fetch the record using the Repository
			var record = await _repo.GetByIdAsync(id);

			// If the repo throws a KeyNotFoundException in GetByIdAsync, 
			// you might not even need this null check depending on your repo design!
			if (record == null) return false;

			// 2. Apply the Business Logic (The Soft Delete)
			record.IsDeleted = true;
			record.UpdateDate = DateTime.UtcNow;

			// 3. Save the changes back using the Repository's Update method
			await _repo.UpdateAsync(record);

			return true;
		}
	}
}