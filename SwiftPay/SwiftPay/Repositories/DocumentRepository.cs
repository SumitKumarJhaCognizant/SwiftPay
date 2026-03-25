using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SwiftPay.Configuration;
using SwiftPay.Models;
using SwiftPay.Repositories.Interfaces;

namespace SwiftPay.Repositories
{
    public class DocumentRepository : IDocumentRepository
    {
        private readonly AppDbContext _db;

        public DocumentRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task<Document> CreateAsync(Document doc)
        {
            await _db.Set<Document>().AddAsync(doc);
            await _db.SaveChangesAsync();
            return doc;
        }

        public async Task<Document?> GetByIdAsync(int documentId)
        {
            return await _db.Set<Document>().FindAsync(documentId);
        }

        public async Task<List<Document>> GetByRemitIdAsync(int remitId)
        {
            return await _db.Set<Document>().Where(d => d.RemitId == remitId && !d.IsDeleted).ToListAsync();
        }

        public async Task<List<Document>> GetAllAsync()
        {
            return await _db.Set<Document>().Where(d => !d.IsDeleted).ToListAsync();
        }

        public async Task UpdateAsync(Document doc)
        {
            _db.Set<Document>().Update(doc);
            await _db.SaveChangesAsync();
        }

        public async Task DeleteAsync(int documentId)
        {
            var entity = await GetByIdAsync(documentId);
            if (entity == null) return;

            // soft delete
            entity.IsDeleted = true;
            entity.UpdateDate = System.DateTime.UtcNow;
            _db.Set<Document>().Update(entity);
            await _db.SaveChangesAsync();

        }
    }
}
