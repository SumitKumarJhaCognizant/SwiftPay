using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SwiftPay.Constants.Enums;
using SwiftPay.DTOs.KycAuditRecordDTO;
using SwiftPay.Repositories.Interfaces;
using SwiftPay.Services.Interfaces;

namespace SwiftPay.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class KYCDocumentsController : ControllerBase
    {
        private readonly IKYCDocumentService _service;
        private readonly IKYCRecordRepository _kycRepo;
        private readonly IWebHostEnvironment _env;

        // Storage folder is created on startup if missing.
        private string StorageRoot => Path.Combine(_env.ContentRootPath, "Storage", "kyc");

        // Phase-1 file size cap: 10 MB per document.
        private const long MaxFileBytes = 10 * 1024 * 1024;

        // Allowed extensions for KYC docs.
        private static readonly string[] AllowedExtensions = { ".pdf", ".jpg", ".jpeg", ".png" };

        public KYCDocumentsController(IKYCDocumentService service, IKYCRecordRepository kycRepo, IWebHostEnvironment env)
        {
            _service = service;
            _kycRepo = kycRepo;
            _env = env;
        }

        // Helper: ensure caller may operate on the given KYC record
        private async Task<(bool ok, IActionResult? error, int kycUserId)> AuthorizeKycAccess(int kycId)
        {
            var kyc = await _kycRepo.GetByIdAsync(kycId);
            if (kyc == null) return (false, NotFound(new { message = "KYC record not found." }), 0);

            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int.TryParse(userIdStr, out var currentUserId);

            // Admin / Compliance / Agent: allow
            if (User.IsInRole("Admin") || User.IsInRole("Compliance") || User.IsInRole("Agent"))
                return (true, null, kyc.UserID);

            // Customer: only own records
            if (kyc.UserID != currentUserId)
                return (false, Forbid(), kyc.UserID);

            return (true, null, kyc.UserID);
        }

        // Legacy: create from a JSON URI body (kept for backward compatibility / testing).
        [HttpPost]
        [Authorize(Roles = "Customer,Agent,Admin")]
        public async Task<IActionResult> Create([FromBody] CreateKYCDocumentDto dto)
        {
            if (dto == null) return BadRequest(new { message = "Body is required." });
            if (string.IsNullOrWhiteSpace(dto.FileURI)) return BadRequest(new { message = "FileURI is required." });

            var (ok, error, _) = await AuthorizeKycAccess(dto.KYCID);
            if (!ok) return error!;

            try
            {
                var created = await _service.CreateAsync(dto);
                return Ok(new { message = "KYC document recorded.", data = created });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to record KYC document.", error = ex.Message });
            }
        }

        // Real upload: multipart form with the actual file. Saves to Storage/kyc and stores the
        // generated filename in FileURI. The file is later served via /api/kycdocuments/{id}/file.
        [HttpPost("upload")]
        [Authorize(Roles = "Customer,Agent,Admin")]
        [RequestSizeLimit(MaxFileBytes)]
        public async Task<IActionResult> Upload(
            [FromForm] IFormFile file,
            [FromForm] int kycId,
            [FromForm] KYCDocumentType docType,
            [FromForm] string? notes)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { message = "File is required." });
            if (file.Length > MaxFileBytes)
                return BadRequest(new { message = $"File too large. Max {MaxFileBytes / (1024 * 1024)} MB." });

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (string.IsNullOrEmpty(ext) || !AllowedExtensions.Contains(ext))
                return BadRequest(new { message = "Only PDF, JPG, JPEG, PNG files are allowed." });

            var (ok, error, _) = await AuthorizeKycAccess(kycId);
            if (!ok) return error!;

            try
            {
                Directory.CreateDirectory(StorageRoot);

                var safeName = Path.GetFileName(file.FileName).Replace(" ", "_");
                var storedName = $"{Guid.NewGuid():N}_{safeName}";
                var fullPath = Path.Combine(StorageRoot, storedName);

                await using (var stream = System.IO.File.Create(fullPath))
                {
                    await file.CopyToAsync(stream);
                }

                var dto = new CreateKYCDocumentDto
                {
                    KYCID = kycId,
                    DocType = docType,
                    FileURI = storedName,
                    Notes = notes,
                };

                var created = await _service.CreateAsync(dto);
                return Ok(new { message = "Document uploaded successfully.", data = created });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Upload failed.", error = ex.Message });
            }
        }

        // Stream the underlying file to the caller (with auth + ownership check).
        [HttpGet("{id:int}/file")]
        public async Task<IActionResult> DownloadFile(int id)
        {
            var doc = await _service.GetByIdAsync(id);
            if (doc == null) return NotFound(new { message = "KYC document not found." });

            var (ok, error, _) = await AuthorizeKycAccess(doc.KYCID);
            if (!ok) return error!;

            if (string.IsNullOrWhiteSpace(doc.FileURI))
                return NotFound(new { message = "File reference is empty." });

            // Reject anything that looks like a URL — these are legacy URI records, not on-disk files.
            if (doc.FileURI.Contains("://") || doc.FileURI.StartsWith("/"))
                return BadRequest(new { message = "This document is a URL reference, not an uploaded file." });

            var fullPath = Path.Combine(StorageRoot, doc.FileURI);
            var fullPathResolved = Path.GetFullPath(fullPath);
            var rootResolved = Path.GetFullPath(StorageRoot);

            // Path traversal guard.
            if (!fullPathResolved.StartsWith(rootResolved, StringComparison.OrdinalIgnoreCase))
                return BadRequest(new { message = "Invalid file path." });

            if (!System.IO.File.Exists(fullPathResolved))
                return NotFound(new { message = "File not found on disk." });

            var contentType = GetContentType(Path.GetExtension(fullPathResolved));
            var stream = System.IO.File.OpenRead(fullPathResolved);

            // Inline content-disposition so PDFs / images render in a browser tab.
            return File(stream, contentType, fileDownloadName: null, enableRangeProcessing: true);
        }

        private static string GetContentType(string ext) => ext.ToLowerInvariant() switch
        {
            ".pdf"  => "application/pdf",
            ".jpg"  => "image/jpeg",
            ".jpeg" => "image/jpeg",
            ".png"  => "image/png",
            _       => "application/octet-stream",
        };

        [HttpGet("kyc/{kycId:int}")]
        public async Task<IActionResult> GetByKycId(int kycId)
        {
            var (ok, error, _) = await AuthorizeKycAccess(kycId);
            if (!ok) return error!;

            var list = await _service.GetByKycIdAsync(kycId);
            return Ok(new { message = "KYC documents retrieved.", data = list });
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var doc = await _service.GetByIdAsync(id);
            if (doc == null) return NotFound(new { message = "KYC document not found." });

            var (ok, error, _) = await AuthorizeKycAccess(doc.KYCID);
            if (!ok) return error!;

            return Ok(new { message = "KYC document retrieved.", data = doc });
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Compliance")]
        public async Task<IActionResult> GetAll()
        {
            var list = await _service.GetAllAsync();
            return Ok(new { message = "KYC documents retrieved.", data = list });
        }

        [HttpPatch("{id:int}/verify")]
        [Authorize(Roles = "Admin,Compliance")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateKYCDocumentStatusDto dto)
        {
            try
            {
                var updated = await _service.UpdateStatusAsync(id, dto);
                if (updated == null) return NotFound(new { message = "KYC document not found." });
                return Ok(new { message = "KYC document status updated.", data = updated });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to update.", error = ex.Message });
            }
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Customer,Admin,Compliance")]
        public async Task<IActionResult> Delete(int id)
        {
            var doc = await _service.GetByIdAsync(id);
            if (doc == null) return NotFound(new { message = "KYC document not found." });

            var (ok, error, _) = await AuthorizeKycAccess(doc.KYCID);
            if (!ok) return error!;

            // Best-effort: delete the underlying file too if it lives in our storage.
            if (!string.IsNullOrWhiteSpace(doc.FileURI) && !doc.FileURI.Contains("://") && !doc.FileURI.StartsWith("/"))
            {
                var fullPath = Path.Combine(StorageRoot, doc.FileURI);
                var fullResolved = Path.GetFullPath(fullPath);
                var rootResolved = Path.GetFullPath(StorageRoot);
                if (fullResolved.StartsWith(rootResolved, StringComparison.OrdinalIgnoreCase) &&
                    System.IO.File.Exists(fullResolved))
                {
                    try { System.IO.File.Delete(fullResolved); } catch { /* ignore */ }
                }
            }

            var deleted = await _service.DeleteAsync(id);
            if (!deleted) return NotFound(new { message = "KYC document not found." });
            return Ok(new { message = "KYC document deleted." });
        }
    }
}
