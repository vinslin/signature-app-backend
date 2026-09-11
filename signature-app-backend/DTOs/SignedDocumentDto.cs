namespace signature_app_backend.DTOs
{
    public class SignedDocumentDto
    {
        public int Id { get; set; }
        public string DocumentName { get; set; } = string.Empty;
        public string? SignedBy { get; set; }
        public DateTime SignedDate { get; set; }
    }
}
