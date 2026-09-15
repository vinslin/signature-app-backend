using Microsoft.AspNetCore.Mvc;
using signature_app_backend.DTOs;
using signature_app_backend.Services;

namespace signature_app_backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ContractsController : ControllerBase
    {
        private readonly IContractService _contractService;
        private readonly IFileStorageService _fileStorageService;
        private readonly ILogger<ContractsController> _logger;

        public ContractsController(
            IContractService contractService,
            IFileStorageService fileStorageService,
            ILogger<ContractsController> logger)
        {
            _contractService = contractService;
            _fileStorageService = fileStorageService;
            _logger = logger;
        }

        /// <summary>
        /// Returns the original PDF contract/template as a binary application/pdf response,
        /// streamed from file storage (not a database binary column).
        /// </summary>
        [HttpGet("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetContract(int id)
        {
            try
            {
                var contract = await _contractService.GetContractByIdAsync(id);

                if (contract == null)
                {
                    return NotFound(new { message = $"Contract with Id {id} was not found." });
                }

                var stream = await _fileStorageService.GetAsync(contract.FilePath);

                if (stream == null)
                {
                    return NotFound(new { message = $"Contract file for Id {id} was not found on disk." });
                }

                return File(stream, "application/pdf", contract.ContractName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving contract {ContractId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "Error retrieving the contract.", error = ex.Message });
            }
        }

        /// <summary>
        /// Accepts a PDF contract template and stores it via IFileStorageService.
        /// The multipart/form-data request includes:
        /// - file: The PDF template as binary data
        /// - contractName: Display name for the contract
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(ContractDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ContractDto>> UploadContract([FromForm] UploadContractRequest request)
        {
            try
            {
                var file = request.File;

                if (file == null || file.Length == 0)
                {
                    return BadRequest(new { message = "File is required and cannot be empty." });
                }

                if (!file.ContentType.Equals("application/pdf", StringComparison.OrdinalIgnoreCase))
                {
                    return BadRequest(new { message = "Only PDF files are accepted." });
                }

                if (string.IsNullOrWhiteSpace(request.ContractName))
                {
                    return BadRequest(new { message = "Contract name is required." });
                }

                await using var stream = file.OpenReadStream();
                var contract = await _contractService.SaveContractAsync(request.ContractName, stream, file.FileName);

                _logger.LogInformation("Contract saved: {ContractName}", contract.ContractName);

                var result = new ContractDto
                {
                    Id = contract.Id,
                    ContractName = contract.ContractName,
                    CreatedDate = contract.CreatedDate
                };

                return CreatedAtAction(nameof(GetContract), new { id = result.Id }, result);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Validation error when uploading contract");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading contract");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "Error saving the contract.", error = ex.Message });
            }
        }
    }
}
