using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SwiftPay.Configuration;
using SwiftPay.Domain.Remittance.Entities;
using SwiftPay.Models;
using SwiftPay.Repositories.Interfaces;

namespace SwiftPay.Repositories
{
	public class RemittanceRepository : IRemittanceRepository
	{
		private readonly AppDbContext _db;

		public RemittanceRepository(AppDbContext db)
		{
			_db = db;
		}

		/// <summary>
		/// Retrieves remittances for a given customer with pagination.
		/// </summary>
		public async Task<List<RemittanceRequest>> GetByCustomerIdAsync(int customerId, int page, int limit, string? status = null)
		{
			if (page <= 0) page = 1;
			if (limit <= 0) limit = 10;

			var query = _db.Set<RemittanceRequest>().Where(r => r.CustomerId == customerId && !r.IsDeleted);

			if (!string.IsNullOrWhiteSpace(status))
			{
				// status stored as string in DB via conversion usually, compare enum names
				query = query.Where(r => r.Status.ToString() == status);
			}

			return await query.OrderByDescending(r => r.CreatedDate)
				.Skip((page - 1) * limit)
				.Take(limit)
				.ToListAsync();
		}

		/// <summary>
		/// Creates a new remittance request and saves it to the database.
		/// </summary>
		public async Task<RemittanceRequest> CreateAsync(RemittanceRequest entity)
		{
			await _db.Set<RemittanceRequest>().AddAsync(entity);
			var affected = await _db.SaveChangesAsync();

			if (affected == 0)
				throw new InvalidOperationException("Insert failed; no rows were affected.");

			return entity;
		}

		/// <summary>
		/// Retrieves a remittance request by its RemitId (string).
		/// Includes related documents and validations.
		/// </summary>
        public async Task<RemittanceRequest?> GetByIdAsync(int remitId)
		{
            return await _db.Set<RemittanceRequest>()
                .Include(r => r.Documents)
                .Include(r => r.Validations)
                .FirstOrDefaultAsync(r => r.RemitId == remitId && !r.IsDeleted);
		}

		/// <summary>
		/// Updates an existing remittance request.
		/// </summary>
		public async Task UpdateAsync(RemittanceRequest remittance)
		{
			_db.Set<RemittanceRequest>().Update(remittance);

			var affected = await _db.SaveChangesAsync();
			if (affected == 0)
				throw new InvalidOperationException("Update failed; no rows were affected.");
		}

		/// <summary>
		/// Persists a list of validation results for a remittance.
		/// </summary>
		public async Task AddValidationsAsync(List<RemitValidation> validations)
		{
			if (validations == null || validations.Count == 0)
				return;

			await _db.Set<RemitValidation>().AddRangeAsync(validations);

			var affected = await _db.SaveChangesAsync();
			if (affected == 0)
				throw new InvalidOperationException("Validation insert failed.");
		}

        /// <summary>
        /// Retrieves all validation records for a remittance.
        /// </summary>
        public async Task<List<RemitValidation>> GetValidationsByRemitIdAsync(int remitId)
        {
            return await _db.Set<RemitValidation>()
                .Where(v => v.RemitId == remitId)
                .OrderBy(v => v.CheckedDate)
                .ToListAsync();
        }

        /// <summary>
        /// Retrieves all remittance requests.
        /// </summary>
        public async Task<IEnumerable<RemittanceRequest>> GetAllAsync()
        {
            return await _db.Set<RemittanceRequest>().ToListAsync();
        }
	}
}