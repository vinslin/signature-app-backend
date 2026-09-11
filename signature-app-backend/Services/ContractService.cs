using Microsoft.EntityFrameworkCore;
using signature_app_backend.Data;
using signature_app_backend.Models;

namespace signature_app_backend.Services
{
    public interface IContractService
    {
        Task<Contract?> GetContractByIdAsync(int id);
    }

    public class ContractService : IContractService
    {
        private readonly AppDbContext _context;

        public ContractService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Contract?> GetContractByIdAsync(int id)
        {
            return await _context.Contracts
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id);
        }
    }
}
