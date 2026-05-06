using System.Collections.Generic;
using System.Threading.Tasks;
using SwiftPay.Models;

namespace SwiftPay.Repositories.Interfaces
{
    public interface IKYCDocumentRepository
    {
        Task<KYCDocument> CreateAsync(KYCDocument entity);
        Task<KYCDocument?> GetByIdAsync(int kycDocumentId);
        Task<IEnumerable<KYCDocument>> GetByKycIdAsync(int kycId);
        Task<IEnumerable<KYCDocument>> GetAllAsync();
        Task<KYCDocument> UpdateAsync(KYCDocument entity);
        Task<bool> DeleteAsync(int kycDocumentId);
    }
}
