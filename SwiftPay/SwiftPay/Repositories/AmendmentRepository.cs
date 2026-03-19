using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using SwiftPay.Configuration;
using SwiftPay.Domain.Remittance.Entities;
using SwiftPay.Repositories.Interfaces;

namespace SwiftPay.Repositories
{
    public class AmendmentRepository : IAmendmentRepository
    {
        private readonly AppDbContext _db;

        public AmendmentRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task<Amendment> CreateAsync(Amendment entity)
        {
            await _db.Set<Amendment>().AddAsync(entity);
            await _db.SaveChangesAsync();
            return entity;
        }

        public async Task<Amendment?> GetByIdAsync(int id)
        {
            return await _db.Set<Amendment>().FindAsync(id);
        }

        public async Task<IEnumerable<Amendment>> GetAllAsync()
        {
            return await _db.Set<Amendment>().AsQueryable().Where(a => !a.IsDeleted).ToListAsync();
        }

        public async Task<Amendment> UpdateAsync(Amendment entity)
        {
            _db.Set<Amendment>().Update(entity);
            await _db.SaveChangesAsync();
            return entity;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var item = await _db.Set<Amendment>().FindAsync(id);
            if (item == null) return false;
            item.IsDeleted = true;
            _db.Set<Amendment>().Update(item);
            await _db.SaveChangesAsync();
            return true;
        }
    }
}