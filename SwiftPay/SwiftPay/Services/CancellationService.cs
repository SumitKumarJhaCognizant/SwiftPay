using AutoMapper;
using SwiftPay.Domain.Remittance.Entities;
using SwiftPay.DTOs.CancellationDTO;
using SwiftPay.Repositories.Interfaces;
using SwiftPay.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SwiftPay.Services
{
    public class CancellationService : ICancellationService
    {
        private readonly ICancellationRepository _repo;
        private readonly IMapper _mapper;

        public CancellationService(ICancellationRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        public async Task<Cancellation> CreateAsync(CreateCancellationDto dto)
        {
            var entity = _mapper.Map<Cancellation>(dto);
            var created = await _repo.CreateAsync(entity);
            return created;
        }

        public async Task<Cancellation?> GetByIdAsync(int id)
        {
            return await _repo.GetByIdAsync(id);
        }

        public async Task<IEnumerable<Cancellation>> GetAllAsync()
        {
            return await _repo.GetAllAsync();
        }

        public async Task<Cancellation> UpdateAsync(int id, CreateCancellationDto dto)
        {
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) throw new Exception($"Cancellation with ID {id} not found");

            _mapper.Map(dto, existing);
            existing.UpdatedDate = DateTime.UtcNow;

            return await _repo.UpdateAsync(existing);
        }

        public async Task<Cancellation> UpdateStatusAsync(int id, Constants.Enums.CancellationStatus status)
        {
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) throw new KeyNotFoundException($"Cancellation with ID {id} not found");

            existing.Status = status;
            existing.UpdatedDate = DateTime.UtcNow;

            return await _repo.UpdateAsync(existing);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            return await _repo.DeleteAsync(id);
        }
    }
}
