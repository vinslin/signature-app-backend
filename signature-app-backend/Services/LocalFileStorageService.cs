namespace signature_app_backend.Services
{
    /// <summary>
    /// Simulates Azure Blob Storage using local folders on disk. The folders live outside
    /// the backend project directory; their location comes from configuration
    /// (FileStorage:ContractsPath / FileStorage:SavedPdfPath) resolved relative to the
    /// application's content root, never a hardcoded absolute path.
    /// </summary>
    public class LocalFileStorageService : IFileStorageService
    {
        private const string ContractsFolder = "contracts";
        private const string SignedDocumentsFolder = "signeddocuments";

        private readonly string _contractsPath;
        private readonly string _signedDocumentsPath;

        public LocalFileStorageService(IConfiguration configuration, IWebHostEnvironment environment)
        {
            _contractsPath = ResolveConfiguredPath(environment.ContentRootPath, configuration["FileStorage:ContractsPath"], "FileStorage:ContractsPath");
            _signedDocumentsPath = ResolveConfiguredPath(environment.ContentRootPath, configuration["FileStorage:SavedPdfPath"], "FileStorage:SavedPdfPath");

            Directory.CreateDirectory(_contractsPath);
            Directory.CreateDirectory(_signedDocumentsPath);
        }

        public async Task<string> SaveAsync(Stream fileStream, string fileName, string folder)
        {
            var basePath = ResolveFolderBasePath(folder);
            var safeFileName = $"{Guid.NewGuid():N}{Path.GetExtension(fileName)}";
            var fullPath = Path.Combine(basePath, safeFileName);

            await using (var output = new FileStream(fullPath, FileMode.Create, FileAccess.Write))
            {
                await fileStream.CopyToAsync(output);
            }

            return $"{folder}/{safeFileName}";
        }

        public Task<Stream?> GetAsync(string filePath)
        {
            var fullPath = ResolveFullPath(filePath);
            if (!File.Exists(fullPath))
            {
                return Task.FromResult<Stream?>(null);
            }

            Stream stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read);
            return Task.FromResult<Stream?>(stream);
        }

        public Task<bool> ExistsAsync(string filePath)
        {
            return Task.FromResult(File.Exists(ResolveFullPath(filePath)));
        }

        public static string ContractsFolderKey => ContractsFolder;
        public static string SignedDocumentsFolderKey => SignedDocumentsFolder;

        private static string ResolveConfiguredPath(string contentRootPath, string? configuredPath, string configKey)
        {
            if (string.IsNullOrWhiteSpace(configuredPath))
            {
                throw new InvalidOperationException($"Configuration value '{configKey}' is missing.");
            }

            return Path.GetFullPath(Path.Combine(contentRootPath, configuredPath));
        }

        private string ResolveFolderBasePath(string folder) => folder switch
        {
            ContractsFolder => _contractsPath,
            SignedDocumentsFolder => _signedDocumentsPath,
            _ => throw new ArgumentException($"Unknown storage folder '{folder}'.", nameof(folder))
        };

        private string ResolveFullPath(string filePath)
        {
            var separatorIndex = filePath.IndexOf('/');
            if (separatorIndex < 0)
            {
                throw new ArgumentException($"Invalid file path '{filePath}'.", nameof(filePath));
            }

            var folder = filePath[..separatorIndex];
            var fileName = Path.GetFileName(filePath[(separatorIndex + 1)..]);
            var basePath = ResolveFolderBasePath(folder);

            return Path.Combine(basePath, fileName);
        }
    }
}
