using Microsoft.EntityFrameworkCore;
using SwiftPay.Configuration;
using SwiftPay.Domain.Remittance.Entities;
using SwiftPay.Models;
using SwiftPay.Repositories.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SwiftPay.Repositories
{
	public class RemitValidationRepository : IRemitValidationRepository
	{
		private readonly AppDbContext _db;

		public RemitValidationRepository(AppDbContext db)
		{
			_db = db;
		}

		public async Task AddRangeAsync(IEnumerable<RemitValidation> validations)
		{
			await _db.Set<RemitValidation>().AddRangeAsync(validations);
			await _db.SaveChangesAsync();
		}

		public async Task<RemitValidation?> GetByIdAsync(Guid validationId)
		{
			return await _db.Set<RemitValidation>().FindAsync(validationId);
		}

		public async Task UpdateAsync(RemitValidation validation)
		{
			_db.Set<RemitValidation>().Update(validation);
			await _db.SaveChangesAsync();
		}

		public async Task<bool> DeleteAsync(Guid validationId)
		{
			var entity = await GetByIdAsync(validationId);
			if (entity == null) return false;

			entity.IsDeleted = true;
			_db.Set<RemitValidation>().Update(entity);
			await _db.SaveChangesAsync();
			return true;
		}

		public async Task<List<RemitValidation>> GetByRemitIdAsync(int remitId)
		{
			return await _db.Set<RemitValidation>()
				.Where(v => v.RemitId == remitId)
				.ToListAsync();
		}
	}
}