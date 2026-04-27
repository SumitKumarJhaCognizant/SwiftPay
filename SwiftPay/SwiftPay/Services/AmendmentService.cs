using AutoMapper;
using SwiftPay.Domain.Remittance.Entities;
using SwiftPay.DTOs.AmendmentDTO;
using SwiftPay.Repositories.Interfaces;
using SwiftPay.Services.Interfaces;
using System.Threading.Tasks;

namespace SwiftPay.Services
{
    public class AmendmentService : IAmendmentService
    {
        private readonly IAmendmentRepository _repo;
        private readonly IMapper _mapper;

        public AmendmentService(IAmendmentRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        public async Task<Amendment> CreateAsync(CreateAmendmentDto dto)
        {
            var entity = _mapper.Map<Amendment>(dto);
            var created = await _repo.CreateAsync(entity);
            return created;
        }

        public async Task<Amendment?> GetByIdAsync(int id)
        {
            return await _repo.GetByIdAsync(id);
        }

        public async Task<IEnumerable<Amendment>> GetAllAsync()
        {
            return await _repo.GetAllAsync();
        }

        public async Task<Amendment> UpdateAsync(int id, CreateAmendmentDto dto)
        {
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) throw new Exception($"Amendment with ID {id} not found");

            // Map fields from DTO onto existing entity
            _mapper.Map(dto, existing);
            existing.UpdatedDate = DateTime.UtcNow;

            return await _repo.UpdateAsync(existing);
        }

        public async Task<Amendment> UpdateStatusAsync(int id, Constants.Enums.AmendmentStatus status)
        {
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) throw new KeyNotFoundException($"Amendment with ID {id} not found");

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