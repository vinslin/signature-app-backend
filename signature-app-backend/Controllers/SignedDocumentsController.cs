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
        private readonly IFileStorageService _fileStorageService;
        private readonly ILogger<SignedDocumentsController> _logger;

        public SignedDocumentsController(
            ISignedDocumentService signedDocumentService,
            IFileStorageService fileStorageService,
            ILogger<SignedDocumentsController> logger)
        {
            _signedDocumentService = signedDocumentService;
            _fileStorageService = fileStorageService;
            _logger = logger;
        }

        /// <summary>
        /// Returns metadata for every submitted contract (no PDF binary data).
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<SignedDocumentDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<SignedDocumentDto>>> GetSignedDocuments()
        {
            try
            {
                var documents = await _signedDocumentService.GetAllSignedDocumentsAsync();
                return Ok(documents);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving submitted contracts");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "Error retrieving submitted contracts.", error = ex.Message });
            }
        }

        /// <summary>
        /// Returns the submitted PDF as binary data. Used for both viewing
        /// (opened in a new tab) and downloading on the frontend.
        /// </summary>
        [HttpGet("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetSignedDocument(int id)
        {
            try
            {
                var signedDocument = await _signedDocumentService.GetSignedDocumentByIdAsync(id);

                if (signedDocument == null)
                {
                    return NotFound(new { message = $"Signed document with Id {id} was not found." });
                }

                var stream = await _fileStorageService.GetAsync(signedDocument.FilePath);

                if (stream == null)
                {
                    return NotFound(new { message = $"Signed document file for Id {id} was not found on disk." });
                }

                return File(stream, "application/pdf", signedDocument.DocumentName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving signed document {SignedDocumentId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "Error retrieving the signed document.", error = ex.Message });
            }
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

                await using var stream = file.OpenReadStream();

                var result = await _signedDocumentService.SaveSignedDocumentAsync(
                    file.FileName,
                    stream,
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
