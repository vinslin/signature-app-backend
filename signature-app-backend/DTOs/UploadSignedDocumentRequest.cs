using Microsoft.AspNetCore.Mvc;

namespace signature_app_backend.DTOs
{
    public class UploadSignedDocumentRequest
    {
        [FromForm(Name = "file")]
        public IFormFile? File { get; set; }

        [FromForm(Name = "signedBy")]
        public string? SignedBy { get; set; }
    }
}
