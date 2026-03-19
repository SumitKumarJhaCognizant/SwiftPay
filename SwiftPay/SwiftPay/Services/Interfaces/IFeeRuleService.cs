using System.Collections.Generic;
using System.Threading.Tasks;
using SwiftPay.DTOs.FXQuoteDTO;

namespace SwiftPay.Services.Interfaces
{
    public interface IFeeRuleService
    {
        Task<FeeRuleResponseDto> CreateFeeRuleAsync(CreateFeeRuleRequestDto request);
        Task<IEnumerable<FeeRuleResponseDto>> GetActiveFeeRulesAsync();
        Task<FeeRuleResponseDto> UpdateFeeRuleAsync(string id, UpdateFeeRuleRequestDto request);
        Task<bool> DeleteFeeRuleAsync(string id);
        
    }
}