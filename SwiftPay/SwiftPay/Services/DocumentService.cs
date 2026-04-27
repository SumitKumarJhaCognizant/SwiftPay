using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using SwiftPay.Constants.Enums;
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
			// Pure Mapping: DTO -> Entity
			var entity = _mapper.Map<Document>(dto);

			SetSystemDefaults(entity);

			var created = await _repo.CreateAsync(entity);
			return _mapper.Map<DocumentResponseDto>(created);
		}

		public async Task<DocumentResponseDto> CreateFromFormAsync(IFormFile file, CreateDocumentDto dto)
		{
			if (file == null) throw new ArgumentNullException(nameof(file));

			// 1. Business Logic: Handle the File URI
			var fileId = Guid.NewGuid().ToString("N");
			dto.FileURI = $"/mock/uploads/{fileId}_{Path.GetFileName(file.FileName)}";

			// 2. Pure Mapping: DTO -> Entity
			var entity = _mapper.Map<Document>(dto);

			SetSystemDefaults(entity);

			var created = await _repo.CreateAsync(entity);
			return _mapper.Map<DocumentResponseDto>(created);
		}

		public async Task<DocumentResponseDto?> GetByIdAsync(int documentId)
		{
			var entity = await _repo.GetByIdAsync(documentId);
			return entity == null ? null : _mapper.Map<DocumentResponseDto>(entity);
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
			if (entity == null || entity.IsDeleted)
				throw new KeyNotFoundException("Document not found.");

			// Pure Mapping: Copy DTO values onto existing Entity
			_mapper.Map(dto, entity);

			entity.UpdateDate = DateTime.UtcNow;
			await _repo.UpdateAsync(entity);
		}

		public async Task DeleteAsync(int documentId)
		{
			// Repository handles the hard or soft delete logic
			await _repo.DeleteAsync(documentId);
		}

		public async Task UpdateVerificationStatusAsync(int documentId, VerificationStatus status)
		{
			var entity = await _repo.GetByIdAsync(documentId);
			if (entity == null || entity.IsDeleted)
				throw new KeyNotFoundException($"Document {documentId} not found.");

			entity.VerificationStatus = status;
			entity.UpdateDate = DateTime.UtcNow;

			await _repo.UpdateAsync(entity);
		}

		// Helper to keep logic DRY (Don't Repeat Yourself)
		private void SetSystemDefaults(Document entity)
		{
			entity.UploadedDate = DateTimeOffset.UtcNow;
			entity.VerificationStatus = VerificationStatus.Pending;
			entity.CreatedDate = DateTime.UtcNow;
			entity.UpdateDate = DateTime.UtcNow;
			entity.IsDeleted = false;
		}
	}
}