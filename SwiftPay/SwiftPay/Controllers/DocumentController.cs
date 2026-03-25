using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading.Tasks;
using SwiftPay.Services.Interfaces;
using SwiftPay.DTOs.RemittanceDTO;
using System.Linq;

namespace SwiftPay.Controllers
{
    [ApiController]
    [Route("api/documents")]
    public class DocumentController : ControllerBase
    {
        private readonly IDocumentService _documentService;

        public DocumentController(IDocumentService documentService)
        {
            _documentService = documentService;
        }

        [HttpPost]
        [ProducesResponseType(typeof(DocumentResponseDto), StatusCodes.Status201Created)]
        public async Task<IActionResult> Create([FromForm] Microsoft.AspNetCore.Http.IFormFile file, [FromForm] int remitId, [FromForm] int docType)
        {
            if (file == null)
                return BadRequest(new { message = "File is required." });

            var created = await _documentService.CreateFromFormAsync(file, remitId, docType);
            return CreatedAtAction(nameof(GetById), new { id = created.DocumentId }, created);
        }
        /// <summary>
        /// get all the document info
        /// </summary>
        /// <returns></returns>
		//[HttpGet]
		//[ProducesResponseType(typeof(List<DocumentResponseDto>), StatusCodes.Status200OK)]
		//[ProducesResponseType(StatusCodes.Status404NotFound)]
		//public async Task<IActionResult> GetAll()
		//{
		//	var list = await _documentService.GetAllAsync();
		//	return Ok(list);
		//}

		[HttpGet("{id}")]
        [ProducesResponseType(typeof(DocumentResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            var doc = await _documentService.GetByIdAsync(id);
            if (doc == null) return NotFound(new { message = "Document not found." });
            return Ok(doc);
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<DocumentResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetByRemitId([FromQuery] int remitId)
        {
            if (remitId <= 0)
                return BadRequest(new { message = "remitId query parameter is required and must be a positive integer." });

            var list = await _documentService.GetByRemitIdAsync(remitId);
            var activeDocuments = list.Where(doc => !doc.IsDeleted).ToList();
            return Ok(activeDocuments);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateDocumentDto dto)
        {
            if (id != dto.DocumentId) return BadRequest(new { message = "DocumentId mismatch." });
            await _documentService.UpdateAsync(dto);
            return NoContent();
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> Delete(int id)
        {
            var document = await _documentService.GetByIdAsync(id);
            if (document == null || document.IsDeleted)
                return NotFound(new { message = "Document not found or already deleted." });

            var updateDto = new UpdateDocumentDto
            {
                DocumentId = document.DocumentId,
                DocType = document.DocType,
                FileURI = document.FileURI,
                IsDeleted = true
            };

            await _documentService.UpdateAsync(updateDto);
            return NoContent();
        }
    }
}
