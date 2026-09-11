namespace signature_app_backend.Models
{
    public class Customer
    {
        public int Id { get; set; }
        public required string FullName { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? BusinessName { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}
