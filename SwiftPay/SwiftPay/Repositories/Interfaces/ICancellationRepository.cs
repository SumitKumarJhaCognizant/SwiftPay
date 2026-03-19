using System.Collections.Generic;
using SwiftPay.Domain.Remittance.Entities;

namespace SwiftPay.Repositories.Interfaces
{
    public interface ICancellationRepository
    {
        Task<Cancellation> CreateAsync(Cancellation entity);
        Task<Cancellation?> GetByIdAsync(int id);
        Task<IEnumerable<Cancellation>> GetAllAsync();
        Task<Cancellation> UpdateAsync(Cancellation entity);
        Task<bool> DeleteAsync(int id);
    }
}
