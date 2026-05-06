using System.Collections.Generic;
using System.Threading.Tasks;
using SwiftPay.DTOs.KycAuditRecordDTO;

namespace SwiftPay.Services.Interfaces
{
    public interface IKYCDocumentService
    {
        Task<KYCDocumentResponseDto> CreateAsync(CreateKYCDocumentDto dto);
        Task<KYCDocumentResponseDto?> GetByIdAsync(int kycDocumentId);
        Task<IEnumerable<KYCDocumentResponseDto>> GetByKycIdAsync(int kycId);
        Task<IEnumerable<KYCDocumentResponseDto>> GetAllAsync();
        Task<KYCDocumentResponseDto?> UpdateStatusAsync(int kycDocumentId, UpdateKYCDocumentStatusDto dto);
        Task<bool> DeleteAsync(int kycDocumentId);
    }
}
