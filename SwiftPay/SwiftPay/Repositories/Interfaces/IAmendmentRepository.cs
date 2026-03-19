using SwiftPay.Domain.Remittance.Entities;

namespace SwiftPay.Repositories.Interfaces
{
    public interface IAmendmentRepository
    {
        Task<Amendment> CreateAsync(Amendment entity);
        Task<Amendment?> GetByIdAsync(int id);
        Task<IEnumerable<Amendment>> GetAllAsync();
        Task<Amendment> UpdateAsync(Amendment entity);
        Task<bool> DeleteAsync(int id);
    }
}