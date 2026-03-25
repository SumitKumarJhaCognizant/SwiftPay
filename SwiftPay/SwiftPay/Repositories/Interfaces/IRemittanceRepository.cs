using System.Threading.Tasks;
using SwiftPay.Domain.Remittance.Entities;

namespace SwiftPay.Repositories.Interfaces
{
    public interface IRemittanceRepository
    {
        Task<RemittanceRequest> CreateAsync(RemittanceRequest entity);
        
        Task<IEnumerable<RemittanceRequest>> GetAllAsync(); // Added method

        // Get by integer RemitId
        Task<RemittanceRequest?> GetByIdAsync(int remitId);
        
        // Update an existing remittance
        Task UpdateAsync(RemittanceRequest entity);

        // Get remittances for a customer with optional status filter and pagination
        Task<List<RemittanceRequest>> GetByCustomerIdAsync(int customerId, int page, int limit, string? status = null);

        // Validation helpers
        Task AddValidationsAsync(List<SwiftPay.Models.RemitValidation> validations);
        Task<List<SwiftPay.Models.RemitValidation>> GetValidationsByRemitIdAsync(int remitId);
    }
}
