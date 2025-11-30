using DevTrails___BankProject.Data;
using DevTrails___BankProject.Entities;
using DevTrails___BankProject.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DevTrails___BankProject.Repositories
{
    public class TransactionRepository : BaseRepository<Transaction>, ITransactionRepository
    {
        public TransactionRepository(BankContext context) : base(context)
        {
        }

        public async Task AddMultipleTransactionsAsync(params Transaction[] transactions)
        {
            await _dbSet.AddRangeAsync(transactions);
        }

        public async Task<List<Transaction>> GetStatementAsync(Guid accountId, DateTime? start, DateTime? end, int pageNumber, int pageSize)
        {
            var query = _dbSet
                .AsNoTracking()
                .Include(t => t.FromAccount) 
                .Include(t => t.ToAccount)
                .Where(t => t.FromAccountId == accountId || t.ToAccountId == accountId);
            
            if (start.HasValue)
            {
                query = query.Where(t => t.Date >= start.Value.Date);
            }

            if (end.HasValue)
            {
                query = query.Where(t => t.Date < end.Value);
            }

            return await query
                .OrderByDescending(t => t.Date) 
                .Skip((pageNumber - 1) * pageSize)    
                .Take(pageSize)                 
                .ToListAsync();
        }
    }
}
