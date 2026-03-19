using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading.Tasks;
using SwiftPay.Services.Interfaces;
using SwiftPay.DTOs.RemittanceDTO;

namespace SwiftPay.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DocumentController : ControllerBase
    {
        private readonly IDocumentService _documentService;

        public DocumentController(IDocumentService documentService)
        {
            _documentService = documentService;
        }

        [HttpPost]
        [ProducesResponseType(typeof(DocumentResponseDto), StatusCodes.Status201Created)]
        public async Task<IActionResult> Create([FromBody] CreateDocumentDto dto)
        {
            var created = await _documentService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.DocumentId }, created);
        }

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
        public async Task<IActionResult> GetByRemitId([FromQuery] string remitId)
        {
            if (string.IsNullOrWhiteSpace(remitId))
                return BadRequest(new { message = "remitId query parameter is required." });

            var list = await _documentService.GetByRemitIdAsync(remitId);
            return Ok(list);
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
            await _documentService.DeleteAsync(id);
            return NoContent();
        }
    }
}
