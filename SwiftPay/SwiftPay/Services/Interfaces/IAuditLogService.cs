using System.Collections.Generic;
using System.Threading.Tasks;
using SwiftPay.DTOs.UserCustomerDTO;
using System;

namespace SwiftPay.Services.Interfaces
{
    public interface IAuditLogService
    {
        Task<GetAuditLogDto> GetByIdAsync(int auditId);
        Task<IEnumerable<GetAuditLogDto>> GetByUserIdAsync(int userId);
        Task<IEnumerable<GetAuditLogDto>> GetByResourceAsync(string resource);
        Task<IEnumerable<GetAuditLogDto>> GetAllAsync();
        Task<IEnumerable<GetAuditLogDto>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
        
        /// <summary>
        /// Get filtered audit logs with pagination and transformation to DTOs
        /// </summary>
        Task<AuditLogListDto> GetFilteredAsync(
            int? userId = null,
            string? resource = null,
            DateTime? startDate = null,
            DateTime? endDate = null,
            int pageNumber = 1,
            int pageSize = 20);
        
        Task<bool> DeleteAsync(int auditId);
        
        // Allow services to emit audit logs when write operations occur
        Task<GetAuditLogDto> CreateAsync(CreateAuditLogDto dto);
    }
}

