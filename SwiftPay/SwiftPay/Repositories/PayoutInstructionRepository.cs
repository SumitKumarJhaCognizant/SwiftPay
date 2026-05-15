using Microsoft.EntityFrameworkCore;
using SwiftPay.Configuration;
using SwiftPay.Domain.Remittance.Entities;
using SwiftPay.Repositories.Interfaces;

namespace SwiftPay.Repositories
{
	public class PayoutInstructionRepository : IPayoutInstructionRepository
	{
		private readonly AppDbContext _db;
		public PayoutInstructionRepository(AppDbContext db) => _db = db;

		public async Task<PayoutInstruction> AddAsync(PayoutInstruction entity)
		{
			await _db.Set<PayoutInstruction>().AddAsync(entity);
			await _db.SaveChangesAsync();
			return entity;
		}

		public async Task<PayoutInstruction?> GetByIdAsync(string id) =>
			await _db.Set<PayoutInstruction>().FirstOrDefaultAsync(x => x.InstructionId == id && !x.IsDeleted);

		public async Task<IEnumerable<PayoutInstruction>> GetAllAsync() =>
			await _db.Set<PayoutInstruction>().Where(x => !x.IsDeleted).ToListAsync();

		public async Task UpdateAsync(PayoutInstruction entity)
		{
			_db.Entry(entity).State = EntityState.Modified;
			// RowVersion is auto-managed by SQL Server — never try to SET it
			_db.Entry(entity).Property(p => p.RowVersion).IsModified = false;
			await _db.SaveChangesAsync();
		}

		public async Task DeleteAsync(string id)
		{
			var entity = await GetByIdAsync(id);
			if (entity != null)
			{
				entity.IsDeleted = true;
				entity.UpdateDate = DateTimeOffset.UtcNow;
				await _db.SaveChangesAsync();
			}
		}
	}
}