using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using SwiftPay.DTOs.KycAuditRecordDTO;
using SwiftPay.Models;
using SwiftPay.Repositories.Interfaces;
using SwiftPay.Services.Interfaces;

namespace SwiftPay.Services
{
    public class KYCDocumentService : IKYCDocumentService
    {
        private readonly IKYCDocumentRepository _repo;
        private readonly IMapper _mapper;

        public KYCDocumentService(IKYCDocumentRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        public async Task<KYCDocumentResponseDto> CreateAsync(CreateKYCDocumentDto dto)
        {
            var entity = _mapper.Map<KYCDocument>(dto);
            entity.UploadedDate = DateTime.UtcNow;
            entity.CreatedAt = DateTime.UtcNow;
            entity.UpdatedAt = DateTime.UtcNow;
            entity.IsDeleted = false;
            // VerificationStatus uses DB default (Pending) — leave it unset
            var created = await _repo.CreateAsync(entity);
            return _mapper.Map<KYCDocumentResponseDto>(created);
        }

        public async Task<KYCDocumentResponseDto?> GetByIdAsync(int kycDocumentId)
        {
            var entity = await _repo.GetByIdAsync(kycDocumentId);
            return entity == null ? null : _mapper.Map<KYCDocumentResponseDto>(entity);
        }

        public async Task<IEnumerable<KYCDocumentResponseDto>> GetByKycIdAsync(int kycId)
        {
            var list = await _repo.GetByKycIdAsync(kycId);
            return list.Select(d => _mapper.Map<KYCDocumentResponseDto>(d));
        }

        public async Task<IEnumerable<KYCDocumentResponseDto>> GetAllAsync()
        {
            var list = await _repo.GetAllAsync();
            return list.Select(d => _mapper.Map<KYCDocumentResponseDto>(d));
        }

        public async Task<KYCDocumentResponseDto?> UpdateStatusAsync(int kycDocumentId, UpdateKYCDocumentStatusDto dto)
        {
            var entity = await _repo.GetByIdAsync(kycDocumentId);
            if (entity == null) return null;

            entity.VerificationStatus = dto.VerificationStatus;
            if (!string.IsNullOrWhiteSpace(dto.Notes))
                entity.Notes = dto.Notes;
            entity.UpdatedAt = DateTime.UtcNow;

            var updated = await _repo.UpdateAsync(entity);
            return _mapper.Map<KYCDocumentResponseDto>(updated);
        }

        public async Task<bool> DeleteAsync(int kycDocumentId) =>
            await _repo.DeleteAsync(kycDocumentId);
    }
}
