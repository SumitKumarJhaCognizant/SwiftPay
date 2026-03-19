using System.Collections.Generic;
using SwiftPay.Domain.Remittance.Entities;

namespace SwiftPay.Repositories.Interfaces
{
    public interface IRemitReportRepository
    {
        Task<RemitReport> CreateAsync(RemitReport entity);
        Task<RemitReport?> GetByIdAsync(int id);
        Task<IEnumerable<RemitReport>> GetAllAsync();
        Task<RemitReport> UpdateAsync(RemitReport entity);
        Task<bool> DeleteAsync(int id);
    }
}
