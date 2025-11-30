using DevTrails___BankProject.Entities;
using DevTrails___BankProject.Service.Interfaces;
using DevTrails___BankProject.Repositories.Interfaces;
using DevTrails___BankProject.DTOs;
using DevTrails___BankProject.Enums;

namespace DevTrails___BankProject.Service
{
    public class AccountService : IAccountService
    {
        private readonly IAccountRepository _accountRepository;
        private readonly IClientRepository _clientRepository;
        public AccountService(IAccountRepository accountRepository, IClientRepository clientRepository)
        {
          _accountRepository = accountRepository;
          _clientRepository = clientRepository;
        }
        public async Task<AccountViewModel> CreateAccountAsync(AccountCreateModel model)
        {
            var client = await _clientRepository.GetByCpfAsync(model.CPF);
            if (client == null) throw new KeyNotFoundException("Cliente não encontrado.");

            Account newAccount;

            if (model.AccountType == AccountType.Checking)
            {
                if (await _accountRepository.ExistsAccountTypeAsync<CheckingAccount>(client.Id))
                {
                    throw new InvalidOperationException("Cliente já possui Conta Corrente.");
                }    

                newAccount = new CheckingAccount { MonthlyFee = 15.0m };
            }
            else if (model.AccountType == AccountType.Savings)
            {
                if (await _accountRepository.ExistsAccountTypeAsync<SavingsAccount>(client.Id))
                {
                    throw new InvalidOperationException("Cliente já possui Conta Poupança.");
                }  

                newAccount = new SavingsAccount { InterestRate = 0.005m };
            }
            else
            {
                throw new ArgumentException("Tipo de conta inválido.");
            }

            newAccount.AccountID = Guid.NewGuid();
            newAccount.ClientId = client.Id;
            newAccount.AccountStatus = AccountStatus.Active;
            newAccount.Balance = 0;
            newAccount.CreationDate = DateTime.UtcNow;
            newAccount.Number = await GenerateAccountNumberAsync();

            var createdAccount = await _accountRepository.AddAsync(newAccount);
            await _accountRepository.SaveChangesAsync();

            createdAccount.Client = client;
            return AccountViewModel.FromModel(createdAccount);
        }
        public async Task<List<AccountViewModel>> GetAccountsByCpfAsync(string cpf)
        {
            var accounts = await _accountRepository.GetByClientCpfAsync(cpf);

            return accounts.Select(AccountViewModel.FromModel).ToList();
        }
    
        public async Task InactivateAccountAsync(string accountNumber, string userId)
        {
            var account = await _accountRepository.GetByNumberAsync(accountNumber);
            if (account == null) throw new KeyNotFoundException("Conta não encontrada.");

            if (account.Client.UserId != userId)
            {
                throw new UnauthorizedAccessException("Você não tem permissão para alterar esta conta.");
            }

            if (account.AccountStatus != AccountStatus.Active)
            {
                throw new InvalidOperationException("Esta conta já está inativa.");
            }

            if (account.Balance != 0)
            {
                throw new InvalidOperationException("Não é possível inativar uma conta com saldo.");
            }

            account.AccountStatus = AccountStatus.Inactive;
            await _accountRepository.SaveChangesAsync();
        }

        public async Task<AccountViewModel> ReactivateAccountAsync(string accountNumber, string userId)
        {
            var account = await _accountRepository.GetByNumberAsync(accountNumber);
            if (account == null) throw new KeyNotFoundException("Conta não encontrada.");

            if (account.Client.UserId != userId)
            {
                throw new UnauthorizedAccessException("Você não tem permissão para alterar esta conta.");
            }

            if (account.AccountStatus == AccountStatus.Active)
            {
                throw new InvalidOperationException("Esta conta já está ativa.");
            }

            account.AccountStatus = AccountStatus.Active;
            await _accountRepository.SaveChangesAsync();

            return AccountViewModel.FromModel(account);
        }
        private async Task<String> GenerateAccountNumberAsync()
        {
            string agency = "0001";
            string accountNumber = "";

            while (await _accountRepository.ExistsByNumberAsync(accountNumber) || string.IsNullOrEmpty(accountNumber))
            {
                int number = Random.Shared.Next(10000, 99999);
                int digit = Random.Shared.Next(0, 9);

                accountNumber = $"{agency}-{number}-{digit}";
            }

            return accountNumber;
        }
    }
}
