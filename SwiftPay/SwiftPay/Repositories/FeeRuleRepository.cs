using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using SwiftPay.Configuration;
using SwiftPay.FXModule.Api.Models;
using SwiftPay.Repositories.Interfaces;

namespace SwiftPay.Repositories
{
    public class FeeRuleRepository : IFeeRuleRepository
    {
        private readonly AppDbContext _context;

        public FeeRuleRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<FeeRule> AddFeeRuleAsync(FeeRule rule)
        {
            await _context.FeeRules.AddAsync(rule);
            await _context.SaveChangesAsync();
            return rule; 
        }
        public async Task<IEnumerable<FeeRule>> GetAllActiveFeeRulesAsync()
        {
            return await _context.FeeRules
                .Where(f => f.Status == SwiftPay.Constants.Enums.RuleStatus.Active && !f.IsDeleted)
                .ToListAsync();
        }
        public async Task<FeeRule> GetFeeRuleByIdAsync(string ruleId)
        {
            return await _context.FeeRules
                .FirstOrDefaultAsync(f => f.FeeRuleID == ruleId && !f.IsDeleted);
        }

        public async Task UpdateFeeRuleAsync(FeeRule rule)
        {
            _context.FeeRules.Update(rule);
            await _context.SaveChangesAsync();
        }
    }
}