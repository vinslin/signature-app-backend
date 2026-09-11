using signature_app_backend.Data;
using signature_app_backend.DTOs;
using signature_app_backend.Models;

namespace signature_app_backend.Services
{
    public interface ISignedDocumentService
    {
        Task<SignedDocumentDto> SaveSignedDocumentAsync(string documentName, byte[] pdfData, string? signedBy, byte[]? signatureImageData = null);
    }

    public class SignedDocumentService : ISignedDocumentService
    {
        private readonly AppDbContext _context;

        public SignedDocumentService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<SignedDocumentDto> SaveSignedDocumentAsync(
            string documentName,
            byte[] pdfData,
            string? signedBy,
            byte[]? signatureImageData = null)
        {
            if (string.IsNullOrWhiteSpace(documentName))
            {
                throw new ArgumentException("Document name cannot be empty.", nameof(documentName));
            }

            if (pdfData == null || pdfData.Length == 0)
            {
                throw new ArgumentException("PDF data cannot be empty.", nameof(pdfData));
            }

            var signedDocument = new SignedDocument
            {
                DocumentName = documentName.Trim(),
                SignedPdfData = pdfData,
                SignatureImageData = signatureImageData,
                SignedBy = signedBy?.Trim(),
                SignedDate = DateTime.Now
            };

            _context.SignedDocuments.Add(signedDocument);
            await _context.SaveChangesAsync();

            return new SignedDocumentDto
            {
                Id = signedDocument.Id,
                DocumentName = signedDocument.DocumentName,
                SignedBy = signedDocument.SignedBy,
                SignedDate = signedDocument.SignedDate
            };
        }
    }
}
