using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using SwiftPay.Configuration;
using SwiftPay.Domain.Remittance.Entities;
using SwiftPay.Repositories.Interfaces;

namespace SwiftPay.Repositories
{
    public class RefundRefRepository : IRefundRefRepository
    {
        private readonly AppDbContext _db;

        public RefundRefRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task<RefundRef> CreateAsync(RefundRef entity)
        {
            await _db.Set<RefundRef>().AddAsync(entity);
            await _db.SaveChangesAsync();
            return entity;
        }

        public async Task<RefundRef?> GetByIdAsync(int id)
        {
            return await _db.Set<RefundRef>().FindAsync(id);
        }

        public async Task<IEnumerable<RefundRef>> GetAllAsync()
        {
            return await _db.Set<RefundRef>().AsQueryable().Where(r => !r.IsDeleted).ToListAsync();
        }

        public async Task<RefundRef> UpdateAsync(RefundRef entity)
        {
            _db.Set<RefundRef>().Update(entity);
            await _db.SaveChangesAsync();
            return entity;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var item = await _db.Set<RefundRef>().FindAsync(id);
            if (item == null) return false;
            item.IsDeleted = true;
            _db.Set<RefundRef>().Update(item);
            await _db.SaveChangesAsync();
            return true;
        }
    }
}
