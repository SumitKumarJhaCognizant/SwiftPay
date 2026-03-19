using System.Collections.Generic;
using SwiftPay.Domain.Remittance.Entities;
using SwiftPay.DTOs.RefundRefDTO;

namespace SwiftPay.Services.Interfaces
{
    public interface IRefundRefService
    {
        Task<RefundRef> CreateAsync(CreateRefundRefDto dto);
        Task<RefundRef?> GetByIdAsync(int id);
        Task<IEnumerable<RefundRef>> GetAllAsync();
        Task<RefundRef> UpdateAsync(int id, CreateRefundRefDto dto);
        Task<bool> DeleteAsync(int id);
    }
}
