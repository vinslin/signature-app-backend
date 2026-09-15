namespace signature_app_backend.DTOs
{
    public class ContractDto
    {
        public int Id { get; set; }
        public string ContractName { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
    }
}
