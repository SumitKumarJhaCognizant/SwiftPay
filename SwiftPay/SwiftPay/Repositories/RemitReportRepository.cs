using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using SwiftPay.Configuration;
using SwiftPay.Domain.Remittance.Entities;
using SwiftPay.Repositories.Interfaces;

namespace SwiftPay.Repositories
{
    public class RemitReportRepository : IRemitReportRepository
    {
        private readonly AppDbContext _db;

        public RemitReportRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task<RemitReport> CreateAsync(RemitReport entity)
        {
            await _db.Set<RemitReport>().AddAsync(entity);
            await _db.SaveChangesAsync();
            return entity;
        }

        public async Task<RemitReport?> GetByIdAsync(int id)
        {
            return await _db.Set<RemitReport>().FindAsync(id);
        }

        public async Task<IEnumerable<RemitReport>> GetAllAsync()
        {
            return await _db.Set<RemitReport>().AsQueryable().Where(r => !r.IsDeleted).ToListAsync();
        }

        public async Task<RemitReport> UpdateAsync(RemitReport entity)
        {
            _db.Set<RemitReport>().Update(entity);
            await _db.SaveChangesAsync();
            return entity;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var item = await _db.Set<RemitReport>().FindAsync(id);
            if (item == null) return false;
            item.IsDeleted = true;
            _db.Set<RemitReport>().Update(item);
            await _db.SaveChangesAsync();
            return true;
        }
    }
}
