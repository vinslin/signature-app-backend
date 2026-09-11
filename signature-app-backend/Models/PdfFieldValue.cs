namespace signature_app_backend.Models
{
    public class PdfFieldValue
    {
        public int Id { get; set; }
        public required string FieldName { get; set; }
        public string? FieldValue { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}
