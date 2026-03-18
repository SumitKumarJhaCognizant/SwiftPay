using System.Collections.Generic;
using System.Threading.Tasks;
using SwiftPay.Domain.Remittance.Entities;

namespace SwiftPay.Repositories.Interfaces
{
    public interface IComplianceDecisionRepository
    {
        Task<ComplianceDecision> CreateAsync(ComplianceDecision entity);
        Task<IEnumerable<ComplianceDecision>> GetByRemitIdAsync(string remitId);
        Task<ComplianceDecision?> GetByIdAsync(string id);
        Task UpdateAsync(ComplianceDecision entity);
    }
}