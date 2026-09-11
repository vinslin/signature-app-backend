namespace signature_app_backend.Models
{
    public class Contract
    {
        public int Id { get; set; }

        public string ContractName { get; set; } = string.Empty;

        public byte[] PdfData { get; set; } = [];

        public DateTime CreatedDate { get; set; }
    }
}
