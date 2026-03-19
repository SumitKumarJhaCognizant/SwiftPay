using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using SwiftPay.Constants.Enums;
using SwiftPay.DTOs.FXQuoteDTO;
using SwiftPay.FXModule.Api.Models;
using SwiftPay.Repositories.Interfaces;
using SwiftPay.Services.Interfaces;

namespace SwiftPay.Services
{
    public class FeeRuleService : IFeeRuleService
    {
        private readonly IFeeRuleRepository _repo;
        private readonly IMapper _mapper;

        public FeeRuleService(IFeeRuleRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        public async Task<FeeRuleResponseDto> CreateFeeRuleAsync(CreateFeeRuleRequestDto request)
        {
            // 1. Map Request DTO to Database Model
            var newRule = _mapper.Map<FeeRule>(request);
            
            // Set defaults that aren't provided by the user
            newRule.Status = RuleStatus.Active;

            // 2. Save to Database
            var savedRule = await _repo.AddFeeRuleAsync(newRule);

            // 3. Map saved Model back to Response DTO
            return _mapper.Map<FeeRuleResponseDto>(savedRule);
        }
        public async Task<IEnumerable<FeeRuleResponseDto>> GetActiveFeeRulesAsync()
        {
            var rules = await _repo.GetAllActiveFeeRulesAsync();
            return _mapper.Map<IEnumerable<FeeRuleResponseDto>>(rules);
        }
        public async Task<FeeRuleResponseDto> UpdateFeeRuleAsync(string id, UpdateFeeRuleRequestDto request)
        {
            // 1. Find the existing rule
            var existingRule = await _repo.GetFeeRuleByIdAsync(id);
            if (existingRule == null) return null;

            // 2. Map the new values over the existing rule
            _mapper.Map(request, existingRule);
            
            // 3. Update the audit timestamp
            existingRule.UpdateDate = DateTime.UtcNow;

            // 4. Save to database
            await _repo.UpdateFeeRuleAsync(existingRule);

            // 5. Return updated DTO
            return _mapper.Map<FeeRuleResponseDto>(existingRule);
        }

        public async Task<bool> DeleteFeeRuleAsync(string id)
        {
            var existingRule = await _repo.GetFeeRuleByIdAsync(id);
            if (existingRule == null) return false;

            // Perform a "Soft Delete"
            existingRule.IsDeleted = true;
            existingRule.Status = RuleStatus.Inactive;
            existingRule.UpdateDate = DateTime.UtcNow;

            await _repo.UpdateFeeRuleAsync(existingRule);
            return true;
        }
    }
}