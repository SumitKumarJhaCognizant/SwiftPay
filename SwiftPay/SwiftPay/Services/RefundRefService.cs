using AutoMapper;
using SwiftPay.Domain.Remittance.Entities;
using SwiftPay.DTOs.RefundRefDTO;
using SwiftPay.Repositories.Interfaces;
using SwiftPay.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SwiftPay.Services
{
    public class RefundRefService : IRefundRefService
    {
        private readonly IRefundRefRepository _repo;
        private readonly IMapper _mapper;

        public RefundRefService(IRefundRefRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        public async Task<RefundRef> CreateAsync(CreateRefundRefDto dto)
        {
            var entity = _mapper.Map<RefundRef>(dto);
            var created = await _repo.CreateAsync(entity);
            return created;
        }

        public async Task<RefundRef?> GetByIdAsync(int id)
        {
            return await _repo.GetByIdAsync(id);
        }

        public async Task<IEnumerable<RefundRef>> GetAllAsync()
        {
            return await _repo.GetAllAsync();
        }

        public async Task<RefundRef> UpdateAsync(int id, CreateRefundRefDto dto)
        {
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) throw new Exception($"RefundRef with ID {id} not found");

            _mapper.Map(dto, existing);
            existing.UpdatedDate = DateTime.UtcNow;

            return await _repo.UpdateAsync(existing);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            return await _repo.DeleteAsync(id);
        }
    }
}
