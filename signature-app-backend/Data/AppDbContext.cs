using Microsoft.EntityFrameworkCore;
using signature_app_backend.Models;

namespace signature_app_backend.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<PdfFieldValue> PdfFieldValues { get; set; }
        public DbSet<SignedDocument> SignedDocuments { get; set; }
        public DbSet<Customer> Customers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<PdfFieldValue>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.FieldName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.FieldValue).HasColumnType("nvarchar(max)");
                entity.Property(e => e.CreatedDate).HasDefaultValueSql("GETDATE()");
            });

            modelBuilder.Entity<SignedDocument>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.DocumentName).IsRequired().HasMaxLength(255);
                entity.Property(e => e.SignedPdfData).HasColumnType("varbinary(max)");
                entity.Property(e => e.SignatureImageData).HasColumnType("varbinary(max)");
                entity.Property(e => e.SignedBy).HasMaxLength(100);
                entity.Property(e => e.SignedDate).HasDefaultValueSql("GETDATE()");
            });

            modelBuilder.Entity<Customer>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.FullName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Email).HasMaxLength(100);
                entity.Property(e => e.Phone).HasMaxLength(20);
                entity.Property(e => e.BusinessName).HasMaxLength(255);
                entity.Property(e => e.CreatedDate).HasDefaultValueSql("GETDATE()");
            });
        }
    }
}
