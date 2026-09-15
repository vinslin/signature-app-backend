using Microsoft.EntityFrameworkCore;
using signature_app_backend.Data;
using signature_app_backend.DTOs;
using signature_app_backend.Models;

namespace signature_app_backend.Services
{
    public interface ISignedDocumentService
    {
        Task<SignedDocumentDto> SaveSignedDocumentAsync(string documentName, Stream fileStream, string? signedBy);

        Task<IEnumerable<SignedDocumentDto>> GetAllSignedDocumentsAsync();

        Task<SignedDocument?> GetSignedDocumentByIdAsync(int id);
    }

    public class SignedDocumentService : ISignedDocumentService
    {
        private readonly AppDbContext _context;
        private readonly IFileStorageService _fileStorageService;

        public SignedDocumentService(AppDbContext context, IFileStorageService fileStorageService)
        {
            _context = context;
            _fileStorageService = fileStorageService;
        }

        public async Task<SignedDocumentDto> SaveSignedDocumentAsync(
            string documentName,
            Stream fileStream,
            string? signedBy)
        {
            if (string.IsNullOrWhiteSpace(documentName))
            {
                throw new ArgumentException("Document name cannot be empty.", nameof(documentName));
            }

            var filePath = await _fileStorageService.SaveAsync(fileStream, documentName, "signeddocuments");

            var signedDocument = new SignedDocument
            {
                DocumentName = documentName.Trim(),
                FilePath = filePath,
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

        /// <summary>
        /// Returns metadata only for every submitted contract. The projection
        /// is applied in the query itself so FilePath is never exposed to the list view.
        /// </summary>
        public async Task<IEnumerable<SignedDocumentDto>> GetAllSignedDocumentsAsync()
        {
            return await _context.SignedDocuments
                .OrderByDescending(d => d.SignedDate)
                .Select(d => new SignedDocumentDto
                {
                    Id = d.Id,
                    DocumentName = d.DocumentName,
                    SignedBy = d.SignedBy,
                    SignedDate = d.SignedDate
                })
                .ToListAsync();
        }

        /// <summary>
        /// Returns the entity (including FilePath) for a single signed document —
        /// used by the view/download endpoint to resolve the file via IFileStorageService.
        /// </summary>
        public async Task<SignedDocument?> GetSignedDocumentByIdAsync(int id)
        {
            return await _context.SignedDocuments
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.Id == id);
        }
    }
}
