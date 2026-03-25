using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using SwiftPay.DTOs.RemittanceDTO;
using SwiftPay.Models;
using SwiftPay.Repositories.Interfaces;
using SwiftPay.Services.Interfaces;

namespace SwiftPay.Services
{
    public class DocumentService : IDocumentService
    {
        private readonly IDocumentRepository _repo;
        private readonly IMapper _mapper;

        public DocumentService(IDocumentRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        public async Task<DocumentResponseDto> CreateAsync(CreateDocumentDto dto)
        {
            var entity = _mapper.Map<Document>(dto);
            entity.UploadedDate = System.DateTimeOffset.UtcNow;
            entity.CreatedDate = System.DateTime.UtcNow;
            entity.UpdateDate = System.DateTime.UtcNow;
            var created = await _repo.CreateAsync(entity);
            return _mapper.Map<DocumentResponseDto>(created);
        }

        public async Task<DocumentResponseDto> CreateFromFormAsync(Microsoft.AspNetCore.Http.IFormFile file, int remitId, int docType)
        {
            if (file == null) throw new System.ArgumentNullException(nameof(file));

            // Mock storage path for phase-1
            var fileId = System.Guid.NewGuid().ToString("N");
            var safeFileName = System.IO.Path.GetFileName(file.FileName);
            var fileUri = $"/mock/uploads/{fileId}_{safeFileName}";

            var entity = new Document
            {
                RemitId = remitId,
                DocType = (SwiftPay.Constants.Enums.DocumentType)docType,
                FileURI = fileUri,
                UploadedDate = System.DateTimeOffset.UtcNow,
                VerificationStatus = SwiftPay.Constants.Enums.VerificationStatus.Pending,
                CreatedDate = System.DateTime.UtcNow,
                UpdateDate = System.DateTime.UtcNow,
                IsDeleted = false
            };

            var created = await _repo.CreateAsync(entity);
            return _mapper.Map<DocumentResponseDto>(created);
        }

        public async Task<DocumentResponseDto?> GetByIdAsync(int documentId)
        {
            var entity = await _repo.GetByIdAsync(documentId);
            if (entity == null) return null;
            return _mapper.Map<DocumentResponseDto>(entity);
        }

        public async Task<List<DocumentResponseDto>> GetByRemitIdAsync(int remitId)
        {
            var list = await _repo.GetByRemitIdAsync(remitId);
            return _mapper.Map<List<DocumentResponseDto>>(list);
        }

        public async Task<List<DocumentResponseDto>> GetAllAsync()
        {
            var list = await _repo.GetAllAsync();
            return _mapper.Map<List<DocumentResponseDto>>(list);
        }

        public async Task UpdateAsync(UpdateDocumentDto dto)
        {
            var entity = await _repo.GetByIdAsync(dto.DocumentId);
            if (entity == null) throw new System.Exception("Document not found.");

            entity.FileURI = dto.FileURI;
            if (!string.IsNullOrEmpty(dto.VerificationStatus))
                entity.VerificationStatus = (SwiftPay.Constants.Enums.VerificationStatus)System.Enum.Parse(typeof(SwiftPay.Constants.Enums.VerificationStatus), dto.VerificationStatus);

            entity.UpdateDate = System.DateTime.UtcNow;
            entity.IsDeleted = dto.IsDeleted; // Ensure IsDeleted is updated
            await _repo.UpdateAsync(entity);
        }

        public async Task DeleteAsync(int documentId)
        {
            await _repo.DeleteAsync(documentId);
        }
    }
}
