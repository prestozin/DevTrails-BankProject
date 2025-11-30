using DevTrails___BankProject.DTOs;
using DevTrails___BankProject.Entities;

namespace DevTrails___BankProject.Service.Interfaces
{
    public interface IAccountService
    {
        Task<AccountViewModel> CreateAccountAsync(AccountCreateModel dto, string userId);
        Task<List<AccountViewModel>> GetAccountsByCpfAsync(string cpf, string userId);
        Task InactivateAccountAsync(string accountNumber, string userId);
        Task<AccountViewModel> ReactivateAccountAsync(string accountNumber, string userId);
    }
}
