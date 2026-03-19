using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using SwiftPay.Configuration;
using SwiftPay.Domain.Remittance.Entities;
using SwiftPay.Repositories.Interfaces;

namespace SwiftPay.Repositories
{
    public class CancellationRepository : ICancellationRepository
    {
        private readonly AppDbContext _db;

        public CancellationRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task<Cancellation> CreateAsync(Cancellation entity)
        {
            await _db.Set<Cancellation>().AddAsync(entity);
            await _db.SaveChangesAsync();
            return entity;
        }

        public async Task<Cancellation?> GetByIdAsync(int id)
        {
            return await _db.Set<Cancellation>().FindAsync(id);
        }

        public async Task<IEnumerable<Cancellation>> GetAllAsync()
        {
            return await _db.Set<Cancellation>().AsQueryable().Where(c => !c.IsDeleted).ToListAsync();
        }

        public async Task<Cancellation> UpdateAsync(Cancellation entity)
        {
            _db.Set<Cancellation>().Update(entity);
            await _db.SaveChangesAsync();
            return entity;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var item = await _db.Set<Cancellation>().FindAsync(id);
            if (item == null) return false;
            item.IsDeleted = true;
            _db.Set<Cancellation>().Update(item);
            await _db.SaveChangesAsync();
            return true;
        }
    }
}
