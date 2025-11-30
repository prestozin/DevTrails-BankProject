using DevTrails___BankProject.Data;
using DevTrails___BankProject.Entities;
using DevTrails___BankProject.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DevTrails___BankProject.Repositories
{
    public class AccountRepository : BaseRepository<Account>, IAccountRepository
    {
        public AccountRepository(BankContext context) : base(context)
        {
        }

        public async Task<Account?> GetByNumberAsync(string number)
        {
            return await _dbSet 
                .Include(a => a.Client)
                .FirstOrDefaultAsync(a => a.Number == number);
        }

        public async Task<List<Account>> GetByClientCpfAsync(string cpf)
        {
            return await _dbSet
                .Include(a => a.Client) 
                .Where(a => a.Client.CPF == cpf) 
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<bool> ExistsAccountTypeAsync<T>(Guid clientId) where T : Account
        {
            return await _dbSet
                .OfType<T>()
                .AnyAsync(a => a.ClientId == clientId);
        }

        public async Task<bool> ExistsByNumberAsync(string number)
        {
            return await _dbSet.AnyAsync(a => a.Number == number);
        }
    }
}
