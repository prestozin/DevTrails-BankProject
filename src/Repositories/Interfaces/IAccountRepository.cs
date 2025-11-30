using DevTrails___BankProject.Enums;
using DevTrails___BankProject.Entities;

namespace DevTrails___BankProject.Repositories.Interfaces
{
    public interface IAccountRepository : IBaseRepository<Account>
    {
        Task<Account?> GetByNumberAsync(string number);
        Task<List<Account>> GetByClientCpfAsync(string cpf);
        Task<bool> ExistsAccountTypeAsync<T>(Guid clientId) where T : Account;
        Task<bool> ExistsByNumberAsync(string number);
    }
}
