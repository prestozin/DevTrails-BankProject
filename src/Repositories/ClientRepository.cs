using DevTrails___BankProject.Data;
using DevTrails___BankProject.Entities;
using DevTrails___BankProject.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DevTrails___BankProject.Repositories
{
    public class ClientRepository : BaseRepository<Client>, IClientRepository
    {
        public ClientRepository(BankContext context) : base(context)
        {
        }
        public async Task<Client?> GetByCpfAsync(string cpf)
        {
            return await _dbSet 
                .Include(c => c.Accounts)
                .FirstOrDefaultAsync(c => c.CPF == cpf);
        }

        public async Task<bool> ExistsByCpfAsync(string cpf)
        {
            return await _dbSet
                .AnyAsync(c => c.CPF == cpf);
        }
    }
}
