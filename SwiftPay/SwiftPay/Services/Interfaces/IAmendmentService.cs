using SwiftPay.Domain.Remittance.Entities;
using SwiftPay.DTOs.AmendmentDTO;

namespace SwiftPay.Services.Interfaces
{
    public interface IAmendmentService
    {
        Task<Amendment> CreateAsync(CreateAmendmentDto dto);
        Task<Amendment?> GetByIdAsync(int id);
        Task<IEnumerable<Amendment>> GetAllAsync();
        Task<Amendment> UpdateAsync(int id, CreateAmendmentDto dto);
        Task<bool> DeleteAsync(int id);
    }
}