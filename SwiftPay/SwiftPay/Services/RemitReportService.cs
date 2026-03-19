using AutoMapper;
using SwiftPay.Domain.Remittance.Entities;
using SwiftPay.DTOs.RemitReportDTO;
using SwiftPay.Repositories.Interfaces;
using SwiftPay.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SwiftPay.Services
{
    public class RemitReportService : IRemitReportService
    {
        private readonly IRemitReportRepository _repo;
        private readonly IMapper _mapper;

        public RemitReportService(IRemitReportRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        public async Task<RemitReport> CreateAsync(CreateRemitReportDto dto)
        {
            var entity = _mapper.Map<RemitReport>(dto);
            var created = await _repo.CreateAsync(entity);
            return created;
        }

        public async Task<RemitReport?> GetByIdAsync(int id)
        {
            return await _repo.GetByIdAsync(id);
        }

        public async Task<IEnumerable<RemitReport>> GetAllAsync()
        {
            return await _repo.GetAllAsync();
        }

        public async Task<RemitReport> UpdateAsync(int id, CreateRemitReportDto dto)
        {
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) throw new Exception($"RemitReport with ID {id} not found");

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
