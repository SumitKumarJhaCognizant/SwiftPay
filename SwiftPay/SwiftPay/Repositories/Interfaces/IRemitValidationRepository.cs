using SwiftPay.Domain.Remittance.Entities;
using SwiftPay.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SwiftPay.Repositories.Interfaces
{
	public interface IRemitValidationRepository
	{
		Task AddRangeAsync(IEnumerable<RemitValidation> validations);
		Task<List<RemitValidation>> GetByRemitIdAsync(int remitId);
		Task<RemitValidation?> GetByIdAsync(Guid validationId);
		Task UpdateAsync(RemitValidation validation);
		Task<bool> DeleteAsync(Guid validationId);
	}
}