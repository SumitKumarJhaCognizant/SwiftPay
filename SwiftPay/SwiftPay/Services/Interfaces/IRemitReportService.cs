using System.Collections.Generic;
using SwiftPay.Domain.Remittance.Entities;
using SwiftPay.DTOs.RemitReportDTO;

namespace SwiftPay.Services.Interfaces
{
    public interface IRemitReportService
    {
        Task<RemitReport> CreateAsync(CreateRemitReportDto dto);
        Task<RemitReport?> GetByIdAsync(int id);
        Task<IEnumerable<RemitReport>> GetAllAsync();
        Task<RemitReport> UpdateAsync(int id, CreateRemitReportDto dto);
        Task<bool> DeleteAsync(int id);
    }
}
