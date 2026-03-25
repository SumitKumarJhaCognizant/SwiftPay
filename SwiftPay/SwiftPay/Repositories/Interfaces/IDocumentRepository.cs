using System.Collections.Generic;
using System.Threading.Tasks;
using SwiftPay.Models;

namespace SwiftPay.Repositories.Interfaces
{
    public interface IDocumentRepository
    {
        Task<Document> CreateAsync(Document doc);
        Task<Document?> GetByIdAsync(int documentId);
        Task<List<Document>> GetByRemitIdAsync(int remitId);
        Task<List<Document>> GetAllAsync();
        Task UpdateAsync(Document doc);
        Task DeleteAsync(int documentId);
    }
}
