using Microsoft.EntityFrameworkCore;
using SwiftPay.Configuration;
using SwiftPay.Constants.Enums;
using SwiftPay.DTOs.SettlementDTO;
using SwiftPay.Models;
using SwiftPay.Repositories.Interfaces;
using SwiftPay.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SwiftPay.Services
{
	public class SettlementBatchService : ISettlementBatchService
	{
		private readonly ISettlementBatchRepository _repo;
		private readonly AppDbContext _context;

		public SettlementBatchService(ISettlementBatchRepository repo, AppDbContext context)
		{
			_repo = repo;
			_context = context;
		}

		public async Task<SettlementBatch> GenerateBatchAsync(GenerateBatchDto dto)
		{
			// 1. Prepare Corridor Filters
			var currencies = dto.Corridor.Split('-');
			string fromCurr = currencies[0].Trim();
			string toCurr = currencies[1].Trim();

			// 2. Fetch Settled Instructions (In-Memory to bypass EF translation bugs)
			var allInstructions = await _context.PayoutInstructions.ToListAsync();

			var settledInstructions = allInstructions
				.Where(p => p.PartnerStatus == PayOutInstructionStatus.Settled
						 && p.SentDate >= dto.PeriodStart
						 && p.SentDate <= dto.PeriodEnd)
				.ToList();

			if (!settledInstructions.Any())
			{
				throw new InvalidOperationException("No settled transactions found for the specified date range.");
			}

			// 3. Fetch Matching Remittance Requests
			var remitIds = settledInstructions.Select(i => i.RemitId.Trim()).ToList();

			var matchingRemits = await _context.RemittanceRequests
				.Where(r => remitIds.Contains(r.RemitId.ToString().Trim())
						 && r.FromCurrency.Trim() == fromCurr
						 && r.ToCurrency.Trim() == toCurr)
				.ToListAsync();

			// 4. Join and Aggregate Data
			var joinedData = settledInstructions
				.Join(matchingRemits,
					  inst => inst.RemitId.Trim(),
					  remit => remit.RemitId.ToString().Trim(),
					  (inst, remit) => new { Instruction = inst, Remit = remit })
				.ToList();

			if (!joinedData.Any())
			{
				throw new InvalidOperationException("No transactions found matching the corridor and status criteria.");
			}

			// 5. Create the Settlement Batch
			var batch = new SettlementBatch
			{
				Corridor = dto.Corridor,
				PeriodStart = dto.PeriodStart.UtcDateTime,
				PeriodEnd = dto.PeriodEnd.UtcDateTime,
				ItemCount = joinedData.Count,
				TotalSendAmount = joinedData.Sum(x => x.Remit.SendAmount),
				TotalPayoutAmount = joinedData.Sum(x => x.Remit.ReceiverAmount ?? 0),
				CreatedDate = DateTime.UtcNow,
				Status = Status.Open
			};

			return await _repo.CreateAsync(batch);
		}

		public async Task<SettlementBatch> UpdateStatusAsync(UpdateBatchStatusDto dto)
		{
			var batch = await _repo.GetByIdAsync(dto.BatchID)
				?? throw new KeyNotFoundException($"Settlement Batch {dto.BatchID} not found.");

			batch.Status = dto.Status;
			return await _repo.UpdateAsync(batch);
		}

		public async Task<SettlementBatch> GetByIdAsync(int id)
		{
			return await _repo.GetByIdAsync(id);
		}

		public async Task<bool> DeleteAsync(int id)
		{
		
			var batch = await _repo.GetByIdAsync(id);
			if (batch == null) return false;

			if (batch.Status == Status.Reconciled)
			{
				throw new InvalidOperationException($"CRITICAL: Batch {id} is already Reconciled. It is locked for auditing and cannot be deleted.");
			}

		
			batch.IsDeleted = true;
			batch.UpdateDate = DateTime.UtcNow;

			await _repo.UpdateAsync(batch);

			

			return true;
		}
	}
}