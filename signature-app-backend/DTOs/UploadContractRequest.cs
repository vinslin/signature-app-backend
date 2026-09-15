using Microsoft.AspNetCore.Mvc;

namespace signature_app_backend.DTOs
{
    public class UploadContractRequest
    {
        [FromForm(Name = "file")]
        public IFormFile? File { get; set; }

        [FromForm(Name = "contractName")]
        public string? ContractName { get; set; }
    }
}
