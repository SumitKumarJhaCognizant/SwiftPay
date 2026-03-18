

using System.Collections.Generic;
using System.Threading.Tasks;
using SwiftPay.Domain.Remittance.Entities;
using SwiftPay.DTOs.ComplianceDTO;

namespace SwiftPay.Services.Interfaces
{
    public interface IComplianceDecisionService
    {
        Task<ComplianceDecision> RecordDecisionAsync(CreateComplianceDecisionDto dto);
        Task<IEnumerable<ComplianceDecision>> GetDecisionsByRemittanceAsync(string remitId);
        Task<ComplianceDecision?> GetDecisionByIdAsync(string id);
        Task<bool> UpdateDecisionAsync(string id, UpdateComplianceDecisionDto dto);
        Task<bool> SoftDeleteDecisionAsync(string id);
    }
}