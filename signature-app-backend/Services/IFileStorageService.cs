namespace signature_app_backend.Services
{
    /// <summary>
    /// Abstraction over physical file storage. Today <see cref="LocalFileStorageService"/>
    /// simulates Azure Blob Storage using local folders; swapping in a future
    /// AzureBlobStorageService requires no changes to controllers or other services.
    /// </summary>
    public interface IFileStorageService
    {
        /// <summary>
        /// Saves the stream under the given logical folder ("contracts" or "signeddocuments")
        /// and returns the relative path to persist in the database.
        /// </summary>
        Task<string> SaveAsync(Stream fileStream, string fileName, string folder);

        /// <summary>
        /// Opens the file at the given relative path (as returned by SaveAsync), or null if it doesn't exist.
        /// </summary>
        Task<Stream?> GetAsync(string filePath);

        Task<bool> ExistsAsync(string filePath);
    }
}
