using signature_app_backend.Data;
using signature_app_backend.DTOs;
using signature_app_backend.Models;
using Microsoft.EntityFrameworkCore;

namespace signature_app_backend.Services
{
    public interface IPdfFieldService
    {
        Task<IEnumerable<PdfFieldValueDto>> GetFieldValuesAsync();
    }

    public class PdfFieldService : IPdfFieldService
    {
        private readonly AppDbContext _context;

        public PdfFieldService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<PdfFieldValueDto>> GetFieldValuesAsync()
        {
            var fields = await _context.PdfFieldValues
                .OrderBy(f => f.Id)
                .ToListAsync();

            return fields.Select(f => new PdfFieldValueDto
            {
                FieldName = f.FieldName,
                FieldValue = f.FieldValue
            }).ToList();
        }
    }
}
