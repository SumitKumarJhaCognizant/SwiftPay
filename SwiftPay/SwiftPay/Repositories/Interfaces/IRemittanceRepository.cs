using System.Threading.Tasks;
using SwiftPay.Domain.Remittance.Entities;
using SwiftPay.Models;

namespace SwiftPay.Repositories.Interfaces
{
	public interface IRemittanceRepository
	{
		Task<RemittanceRequest> CreateAsync(RemittanceRequest entity);

		Task<IEnumerable<RemittanceRequest>> GetAllAsync(); // Added method

		// Get by integer RemitId
		Task<RemittanceRequest?> GetByIdAsync(int remitId);

		/// <summary>
		/// Updates an existing remittance.
		/// </summary>
		/// <param name="entity">The remittance to update.</param>
		Task UpdateAsync(RemittanceRequest entity);

		// Get remittances for a customer with optional status filter and pagination
		Task<List<RemittanceRequest>> GetByCustomerIdAsync(int customerId, int page, int limit, string? status = null);

		// Validation helpers
		Task AddValidationsAsync(List<RemitValidation> validations);
		Task<List<RemitValidation>> GetValidationsByRemitIdAsync(int remitId);
	}
}