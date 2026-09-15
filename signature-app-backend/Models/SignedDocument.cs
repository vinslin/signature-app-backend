namespace signature_app_backend.Models
{
    public class SignedDocument
    {
        public int Id { get; set; }
        public required string DocumentName { get; set; }
        public string FilePath { get; set; } = string.Empty;
        public string? SignedBy { get; set; }
        public DateTime SignedDate { get; set; } = DateTime.Now;
    }
}
