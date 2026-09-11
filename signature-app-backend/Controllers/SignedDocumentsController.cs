using Microsoft.AspNetCore.Mvc;
using signature_app_backend.DTOs;
using signature_app_backend.Services;

namespace signature_app_backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SignedDocumentsController : ControllerBase
    {
        private readonly ISignedDocumentService _signedDocumentService;
        private readonly ILogger<SignedDocumentsController> _logger;

        public SignedDocumentsController(ISignedDocumentService signedDocumentService, ILogger<SignedDocumentsController> logger)
        {
            _signedDocumentService = signedDocumentService;
            _logger = logger;
        }

        /// <summary>
        /// Accepts a signed PDF file and optional metadata, then saves it to the database.
        /// The multipart/form-data request includes:
        /// - file: The signed PDF as binary data
        /// - signedBy: (optional) Name of the person who signed the document
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(SignedDocumentDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<SignedDocumentDto>> UploadSignedDocument(
            [FromForm] UploadSignedDocumentRequest request)
        {
            try
            {
                var file = request.File;

                // Validation
                if (file == null || file.Length == 0)
                {
                    return BadRequest(new { message = "File is required and cannot be empty." });
                }

                if (!file.ContentType.Equals("application/pdf", StringComparison.OrdinalIgnoreCase))
                {
                    return BadRequest(new { message = "Only PDF files are accepted." });
                }

                // Read the file into a byte array
                using var memoryStream = new MemoryStream();
                await file.CopyToAsync(memoryStream);
                var pdfData = memoryStream.ToArray();

                if (pdfData.Length == 0)
                {
                    return BadRequest(new { message = "PDF file cannot be empty." });
                }

                // Save to database
                var result = await _signedDocumentService.SaveSignedDocumentAsync(
                    file.FileName,
                    pdfData,
                    request.SignedBy);

                _logger.LogInformation($"Signed document saved: {result.DocumentName} by {result.SignedBy ?? "Unknown"}");

                return CreatedAtAction(nameof(UploadSignedDocument), new { id = result.Id }, result);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Validation error when uploading signed document");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading signed document");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "Error saving the signed document.", error = ex.Message });
            }
        }
    }
}
