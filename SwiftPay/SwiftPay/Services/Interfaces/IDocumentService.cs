using System.Collections.Generic;
using System.Threading.Tasks;
using SwiftPay.DTOs.RemittanceDTO;

namespace SwiftPay.Services.Interfaces
{
    public interface IDocumentService
    {
        Task<DocumentResponseDto> CreateAsync(CreateDocumentDto dto);
        Task<DocumentResponseDto> CreateFromFormAsync(Microsoft.AspNetCore.Http.IFormFile file, int remitId, int docType);
        Task<DocumentResponseDto?> GetByIdAsync(int documentId);
        Task<List<DocumentResponseDto>> GetByRemitIdAsync(int remitId);
        Task UpdateAsync(UpdateDocumentDto dto);
        Task DeleteAsync(int documentId);
        Task<List<DocumentResponseDto>> GetAllAsync();
    }
}
