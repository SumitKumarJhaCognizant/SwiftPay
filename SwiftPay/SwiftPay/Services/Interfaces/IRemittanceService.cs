using System.Threading.Tasks;
using SwiftPay.Constants.Enums;
using SwiftPay.DTOs.RemittanceDTO;

namespace SwiftPay.Services.Interfaces
{
	public interface IRemittanceService
	{
		Task<CreateRemittanceResponseDto> CreateAsync(CreateRemittanceDto dto);

		// Get by string RemitId (GUID stored as string)
		Task<CreateRemittanceResponseDto?> GetByIdAsync(int remitId);



		// Remit validation functions
		Task<ValidateRemittanceResponseDto> ValidateAsync(int remitId);

		Task<List<RemitValidationDto>> GetValidationsAsync(int remitId);

		// Update and delete remittance
		Task UpdateAsync(int remitId, UpdateRemittanceDto dto); // Updated to use UpdateRemittanceDto
		Task SoftDeleteAsync(int remitId); // Added SoftDeleteAsync method

		// Additional operations
		// Use integer remittance identifier throughout service methods
		Task<string> CancelAsync(int remitId, string cancellationReason);

		Task<List<CreateRemittanceResponseDto>> GetByCustomerRemittancesAsync(int customerId, int page, int limit, string? status = null);

		Task MarkPendingComplianceAsync(int remitId);

		// Update and delete individual validation records
		Task UpdateValidationAsync(RemitValidationDto dto);
		Task DeleteValidationAsync(Guid validationId);

		//Document functions 
		Task<IEnumerable<CreateRemittanceResponseDto>> GetAllAsync(); // Added method
		Task<IEnumerable<RemitValidationDto>> GetValidationsByRemitIdAsync(int remitId); // Added method
		Task<RemitValidationDto> GetValidationByIdAsync(Guid validationId); // Added method

		/// <summary>
		/// Updates the verification status of a remittance by remitId.
		/// </summary>
		/// <param name="remitId">The ID of the remittance.</param>
		/// <param name="status">The new remittance request status.</param>
		Task UpdateVerificationStatusByRemitIdAsync(int remitId, RemittanceRequestStatus status);
	}
}