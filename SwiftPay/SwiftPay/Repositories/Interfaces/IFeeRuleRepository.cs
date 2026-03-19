using System.Threading.Tasks;
using SwiftPay.FXModule.Api.Models;

namespace SwiftPay.Repositories.Interfaces
{
    public interface IFeeRuleRepository
    {
        Task<FeeRule> AddFeeRuleAsync(FeeRule rule);
        Task<IEnumerable<FeeRule>> GetAllActiveFeeRulesAsync();
        Task<FeeRule> GetFeeRuleByIdAsync(string ruleId);
        Task UpdateFeeRuleAsync(FeeRule rule);
    }
}