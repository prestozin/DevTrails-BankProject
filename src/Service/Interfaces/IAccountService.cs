using DevTrails___BankProject.DTOs;
using DevTrails___BankProject.Entities;

namespace DevTrails___BankProject.Service.Interfaces
{
    public interface IAccountService
    {
        Task<AccountViewModel> CreateAccountAsync(AccountCreateModel dto);
        Task<List<AccountViewModel>> GetAccountsByCpfAsync(string cpf);
        Task InactivateAccountAsync(string accountNumber, string userId);
        Task<AccountViewModel> ReactivateAccountAsync(string accountNumber, string userId);
    }
}
