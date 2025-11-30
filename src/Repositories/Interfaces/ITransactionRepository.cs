using DevTrails___BankProject.Entities;

namespace DevTrails___BankProject.Repositories.Interfaces
{
    public interface ITransactionRepository : IBaseRepository<Transaction>
    {
        Task AddMultipleTransactionsAsync(params Transaction[] transactions);
        Task<List<Transaction>> GetStatementAsync(Guid accountId, DateTime? start, DateTime? end, int pageNumber, int pageSize);
    }
}
