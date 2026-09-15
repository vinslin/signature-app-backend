using Microsoft.EntityFrameworkCore;
using signature_app_backend.Data;
using signature_app_backend.Models;

namespace signature_app_backend.Services
{
    public interface IContractService
    {
        Task<Contract?> GetContractByIdAsync(int id);

        Task<Contract> SaveContractAsync(string contractName, Stream fileStream, string fileName);
    }

    public class ContractService : IContractService
    {
        private readonly AppDbContext _context;
        private readonly IFileStorageService _fileStorageService;

        public ContractService(AppDbContext context, IFileStorageService fileStorageService)
        {
            _context = context;
            _fileStorageService = fileStorageService;
        }

        public async Task<Contract?> GetContractByIdAsync(int id)
        {
            return await _context.Contracts
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<Contract> SaveContractAsync(string contractName, Stream fileStream, string fileName)
        {
            if (string.IsNullOrWhiteSpace(contractName))
            {
                throw new ArgumentException("Contract name cannot be empty.", nameof(contractName));
            }

            var filePath = await _fileStorageService.SaveAsync(fileStream, fileName, "contracts");

            var contract = new Contract
            {
                ContractName = contractName.Trim(),
                FilePath = filePath,
                CreatedDate = DateTime.Now
            };

            _context.Contracts.Add(contract);
            await _context.SaveChangesAsync();

            return contract;
        }
    }
}
