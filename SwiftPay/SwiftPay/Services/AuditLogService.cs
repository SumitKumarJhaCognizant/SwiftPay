using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using SwiftPay.Services.Interfaces;
using SwiftPay.Repositories.Interfaces;
using SwiftPay.DTOs.UserCustomerDTO;
using SwiftPay.Domain.Remittance.Entities;

namespace SwiftPay.Services
{
    public class AuditLogService : IAuditLogService
    {
        private readonly IAuditLogRepository _repo;
        private readonly IMapper _mapper;

        public AuditLogService(IAuditLogRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        public async Task<GetAuditLogDto> GetByIdAsync(int auditId)
        {
            var auditLog = await _repo.GetByIdAsync(auditId);
            return _mapper.Map<GetAuditLogDto>(auditLog);
        }

        public async Task<IEnumerable<GetAuditLogDto>> GetByUserIdAsync(int userId)
        {
            var logs = await _repo.GetByUserIdAsync(userId);
            return _mapper.Map<List<GetAuditLogDto>>(logs);
        }

        public async Task<IEnumerable<GetAuditLogDto>> GetByResourceAsync(string resource)
        {
            var logs = await _repo.GetByResourceAsync(resource);
            return _mapper.Map<List<GetAuditLogDto>>(logs);
        }

        public async Task<IEnumerable<GetAuditLogDto>> GetAllAsync()
        {
            var logs = await _repo.GetAllAsync();
            return _mapper.Map<List<GetAuditLogDto>>(logs);
        }

        public async Task<IEnumerable<GetAuditLogDto>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            var logs = await _repo.GetByDateRangeAsync(startDate, endDate);
            return _mapper.Map<List<GetAuditLogDto>>(logs);
        }

        public async Task<AuditLogListDto> GetFilteredAsync(
            int? userId = null,
            string? resource = null,
            DateTime? startDate = null,
            DateTime? endDate = null,
            int pageNumber = 1,
            int pageSize = 20)
        {
            var (logs, totalCount) = await _repo.GetFilteredAsync(userId, resource ?? "", startDate, endDate, pageNumber, pageSize);
            var dtos = _mapper.Map<List<GetAuditLogDto>>(logs);

            return new AuditLogListDto
            {
                AuditLogs = dtos,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task<bool> DeleteAsync(int auditId)
        {
            return await _repo.DeleteAsync(auditId);
        }

        public async Task<GetAuditLogDto> CreateAsync(CreateAuditLogDto dto)
        {
            var entity = _mapper.Map<Domain.Remittance.Entities.AuditLog>(dto);
            entity.Timestamp = dto.Timestamp;
            var created = await _repo.CreateAsync(entity);
            return _mapper.Map<GetAuditLogDto>(created);
        }
    }
}
