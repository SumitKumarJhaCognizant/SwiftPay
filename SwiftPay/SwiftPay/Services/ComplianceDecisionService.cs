

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using SwiftPay.DTOs.ComplianceDTO;
using SwiftPay.Domain.Remittance.Entities;
using SwiftPay.Repositories.Interfaces;
using SwiftPay.Services.Interfaces;

namespace SwiftPay.Services
{
    public class ComplianceDecisionService : IComplianceDecisionService
    {
        private readonly IComplianceDecisionRepository _repo;
        private readonly IMapper _mapper;

        public ComplianceDecisionService(IComplianceDecisionRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        public async Task<ComplianceDecision> RecordDecisionAsync(CreateComplianceDecisionDto dto)
        {
            var decision = _mapper.Map<ComplianceDecision>(dto);
            decision.DecisionId = Guid.NewGuid().ToString();
            decision.DecisionDate = DateTime.UtcNow;
            decision.IsDeleted = false;
            return await _repo.CreateAsync(decision);
        }

        public async Task<IEnumerable<ComplianceDecision>> GetDecisionsByRemittanceAsync(string remitId) =>
            await _repo.GetByRemitIdAsync(remitId);

        public async Task<ComplianceDecision?> GetDecisionByIdAsync(string id) =>
            await _repo.GetByIdAsync(id);

        public async Task<bool> UpdateDecisionAsync(string id, UpdateComplianceDecisionDto dto)
        {
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) return false;

            existing.Decision = dto.Decision;
            existing.Notes = dto.Notes;
            existing.UpdateDate = DateTime.UtcNow;

            await _repo.UpdateAsync(existing);
            return true;
        }

        public async Task<bool> SoftDeleteDecisionAsync(string id)
        {
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) return false;

            existing.IsDeleted = true; // SOFT DELETE FLAG
            existing.UpdateDate = DateTime.UtcNow;

            await _repo.UpdateAsync(existing);
            return true;
        }
    }
}
