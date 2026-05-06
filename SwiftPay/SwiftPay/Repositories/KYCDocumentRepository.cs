using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SwiftPay.Configuration;
using SwiftPay.Models;
using SwiftPay.Repositories.Interfaces;

namespace SwiftPay.Repositories
{
    public class KYCDocumentRepository : IKYCDocumentRepository
    {
        private readonly AppDbContext _db;

        public KYCDocumentRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task<KYCDocument> CreateAsync(KYCDocument entity)
        {
            await _db.Set<KYCDocument>().AddAsync(entity);
            await _db.SaveChangesAsync();
            return entity;
        }

        public async Task<KYCDocument?> GetByIdAsync(int kycDocumentId)
        {
            return await _db.Set<KYCDocument>()
                .FirstOrDefaultAsync(d => d.KYCDocumentId == kycDocumentId && !d.IsDeleted);
        }

        public async Task<IEnumerable<KYCDocument>> GetByKycIdAsync(int kycId)
        {
            return await _db.Set<KYCDocument>()
                .Where(d => d.KYCID == kycId && !d.IsDeleted)
                .OrderBy(d => d.UploadedDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<KYCDocument>> GetAllAsync()
        {
            return await _db.Set<KYCDocument>()
                .Where(d => !d.IsDeleted)
                .ToListAsync();
        }

        public async Task<KYCDocument> UpdateAsync(KYCDocument entity)
        {
            _db.Set<KYCDocument>().Update(entity);
            await _db.SaveChangesAsync();
            return entity;
        }

        public async Task<bool> DeleteAsync(int kycDocumentId)
        {
            var doc = await GetByIdAsync(kycDocumentId);
            if (doc == null) return false;
            doc.IsDeleted = true;
            doc.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return true;
        }
    }
}
