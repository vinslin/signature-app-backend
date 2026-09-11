using Microsoft.AspNetCore.Mvc;
using signature_app_backend.DTOs;
using signature_app_backend.Services;

namespace signature_app_backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PdfFieldsController : ControllerBase
    {
        private readonly IPdfFieldService _pdfFieldService;
        private readonly ILogger<PdfFieldsController> _logger;

        public PdfFieldsController(IPdfFieldService pdfFieldService, ILogger<PdfFieldsController> logger)
        {
            _pdfFieldService = pdfFieldService;
            _logger = logger;
        }

        /// <summary>
        /// Returns all PDF field values from the database.
        /// These are matched against the AcroForm fields in the PDF.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<PdfFieldValueDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<PdfFieldValueDto>>> GetFieldValues()
        {
            try
            {
                var fieldValues = await _pdfFieldService.GetFieldValuesAsync();
                return Ok(fieldValues);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving PDF field values");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "Error retrieving PDF field values.", error = ex.Message });
            }
        }
    }
}
