using System.Collections.Generic;
using SwiftPay.Domain.Remittance.Entities;

namespace SwiftPay.Repositories.Interfaces
{
    public interface IRefundRefRepository
    {
        Task<RefundRef> CreateAsync(RefundRef entity);
        Task<RefundRef?> GetByIdAsync(int id);
        Task<IEnumerable<RefundRef>> GetAllAsync();
        Task<RefundRef> UpdateAsync(RefundRef entity);
        Task<bool> DeleteAsync(int id);
    }
}
