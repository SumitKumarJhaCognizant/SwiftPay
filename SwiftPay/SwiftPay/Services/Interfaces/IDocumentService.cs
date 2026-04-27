using SwiftPay.Constants.Enums;
using SwiftPay.DTOs.RemittanceDTO;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace SwiftPay.Services.Interfaces
{
	public interface IDocumentService
	{
		Task<DocumentResponseDto> CreateAsync(CreateDocumentDto dto);
		// Inside IDocumentService.cs
		Task<DocumentResponseDto> CreateFromFormAsync(IFormFile file, CreateDocumentDto dto);
		Task<DocumentResponseDto?> GetByIdAsync(int documentId);
		Task<List<DocumentResponseDto>> GetByRemitIdAsync(int remitId);
		Task<List<DocumentResponseDto>> GetAllAsync();
		Task UpdateAsync(UpdateDocumentDto dto);
		Task DeleteAsync(int documentId);

		// Ensure the Enum type matches exactly what the Service uses
		Task UpdateVerificationStatusAsync(int documentId, SwiftPay.Constants.Enums.VerificationStatus status);
	}
}