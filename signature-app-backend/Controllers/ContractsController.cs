using Microsoft.AspNetCore.Mvc;
using signature_app_backend.Services;

namespace signature_app_backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ContractsController : ControllerBase
    {
        private readonly IContractService _contractService;
        private readonly ILogger<ContractsController> _logger;

        public ContractsController(IContractService contractService, ILogger<ContractsController> logger)
        {
            _contractService = contractService;
            _logger = logger;
        }

        /// <summary>
        /// Returns the original PDF contract/template as a binary application/pdf response.
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

                return File(contract.PdfData, "application/pdf", contract.ContractName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving contract {ContractId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "Error retrieving the contract.", error = ex.Message });
            }
        }
    }
}
