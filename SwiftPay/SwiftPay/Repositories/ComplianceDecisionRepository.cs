using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SwiftPay.Configuration;
using SwiftPay.Domain.Remittance.Entities;
using SwiftPay.Repositories.Interfaces;

namespace SwiftPay.Repositories
{
    public class ComplianceDecisionRepository : IComplianceDecisionRepository
    {
        private readonly AppDbContext _db;
        public ComplianceDecisionRepository(AppDbContext db) => _db = db;

        public async Task<ComplianceDecision> CreateAsync(ComplianceDecision entity)
        {
            await _db.Set<ComplianceDecision>().AddAsync(entity);
            await _db.SaveChangesAsync();
            return entity;
        }

        public async Task<IEnumerable<ComplianceDecision>> GetByRemitIdAsync(string remitId)
        {
            return await _db.Set<ComplianceDecision>()
                .Where(d => d.RemitId == remitId && !d.IsDeleted) // Filter out soft-deleted
                .ToListAsync();
        }

        public async Task<ComplianceDecision?> GetByIdAsync(string id)
        {
            return await _db.Set<ComplianceDecision>()
                .FirstOrDefaultAsync(d => d.DecisionId == id && !d.IsDeleted);
        }

        public async Task UpdateAsync(ComplianceDecision entity)
        {
            _db.Set<ComplianceDecision>().Update(entity);
            await _db.SaveChangesAsync();
        }
    }
}
