using System.Collections.Generic;
using SwiftPay.Domain.Remittance.Entities;
using SwiftPay.DTOs.CancellationDTO;

namespace SwiftPay.Services.Interfaces
{
    public interface ICancellationService
    {
        Task<Cancellation> CreateAsync(CreateCancellationDto dto);
        Task<Cancellation?> GetByIdAsync(int id);
        Task<IEnumerable<Cancellation>> GetAllAsync();
        Task<Cancellation> UpdateAsync(int id, CreateCancellationDto dto);
        Task<bool> DeleteAsync(int id);
    }
}
