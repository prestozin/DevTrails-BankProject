using DevTrails___BankProject.Data;
using DevTrails___BankProject.DTOs;
using DevTrails___BankProject.Enums;
using DevTrails___BankProject.Entities;
using DevTrails___BankProject.Service.Interfaces;
using DevTrails___BankProject.Repositories.Interfaces;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace DevTrails___BankProject.Service
{
    public class TransactionService : ITransactionService
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly IAccountRepository _accountRepository;
        private readonly IUnitOfWork _unitOfWork;
        public TransactionService(ITransactionRepository transactionRepository, IAccountRepository accountRepository, IUnitOfWork unitOfWork)
        {
            _transactionRepository = transactionRepository;
            _accountRepository = accountRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<TransactionViewModel> DepositAsync(DepositInputModel model)
        {
            await _unitOfWork.BeginTransactionAsync();

            try
            {
                var account = await _accountRepository.GetByNumberAsync(model.AccountNumber);

                if (account == null) throw new KeyNotFoundException("Conta não encontrada.");
                if (account.AccountStatus != AccountStatus.Active) throw new InvalidOperationException("Conta inativa.");

                account.Balance += model.Amount;

                var transaction = new Transaction
                {
                    Id = Guid.NewGuid(),
                    Amount = model.Amount,
                    Date = DateTime.UtcNow,
                    Type = TransactionType.Deposit,
                    Description = "Depósito",
                    ToAccountId = account.AccountID,
                    FromAccountId = null
                };

                var createdTransaction = await _transactionRepository.AddAsync(transaction);
                await _unitOfWork.CommitAsync();

                return TransactionViewModel.FromModel(createdTransaction);
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }

        }

        public async Task<TransactionViewModel> TransferAsync(TransferInputModel model, string userId)
        {
            if (model.FromAccountNumber == model.ToAccountNumber)
            {
                throw new InvalidOperationException("Conta de origem e destino não podem ser iguais.");
            }

            await _unitOfWork.BeginTransactionAsync();

            try
            {
                var fromAccount = await _accountRepository.GetByNumberAsync(model.FromAccountNumber);
                var toAccount = await _accountRepository.GetByNumberAsync(model.ToAccountNumber);

                if (fromAccount == null) throw new KeyNotFoundException($"Origem {model.FromAccountNumber} não encontrada.");
                if (toAccount == null) throw new KeyNotFoundException($"Destino {model.ToAccountNumber} não encontrada.");

                if (fromAccount.Client.UserId != userId)
                {
                    throw new UnauthorizedAccessException("Você não pode transferir de uma conta que não é sua.");
                }

                if (fromAccount.AccountStatus != AccountStatus.Active) throw new InvalidOperationException("Conta de origem inativa.");
                if (toAccount.AccountStatus != AccountStatus.Active) throw new InvalidOperationException("Conta de destino inativa.");

                decimal fee = model.Amount * 0.005m;
                decimal totalDebit = model.Amount + fee;

                if (fromAccount.Balance < totalDebit)
                {
                    throw new InvalidOperationException($"Saldo insuficiente. Saldo necessário: {totalDebit:C}");
                }
                   
                fromAccount.Balance -= totalDebit;
                toAccount.Balance += model.Amount;

                var sentTransaction = new Transaction
                {
                    Id = Guid.NewGuid(),
                    Amount = -model.Amount, 
                    Date = DateTime.UtcNow,
                    Type = TransactionType.TransferSent,                                 
                    Description = $"Transferência enviada para {toAccount.Number}",
                    FromAccountId = fromAccount.AccountID,
                    ToAccountId = toAccount.AccountID 
                };

                var receivedTransaction = new Transaction
                {
                    Id = Guid.NewGuid(),
                    Amount = model.Amount, 
                    Date = DateTime.UtcNow,
                    Type = TransactionType.TransferReceived, 
                    Description = $"Transferência recebida de {fromAccount.Number}",
                    ToAccountId = toAccount.AccountID
                };

                var feeTransaction = new Transaction
                {
                    Id = Guid.NewGuid(),
                    Amount = -fee,
                    Date = DateTime.UtcNow,
                    Type = TransactionType.ServiceFee,
                    Description = "Taxa de Transferência",
                    FromAccountId = fromAccount.AccountID
                };

                await _transactionRepository.AddMultipleTransactionsAsync(sentTransaction, receivedTransaction, feeTransaction);

                await _unitOfWork.CommitAsync();

                return TransactionViewModel.FromModel(sentTransaction);
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw; 
            }
        }

        public async Task<TransactionViewModel> WithdrawAsync(WithdrawInputModel model, string userId)
        {
            await _unitOfWork.BeginTransactionAsync();

            try
            {
                var account = await _accountRepository.GetByNumberAsync(model.AccountNumber);

                if (account == null) throw new KeyNotFoundException("Conta não encontrada.");
                if (account.AccountStatus != AccountStatus.Active) throw new Exception("Conta Inativa");

                if (account.Client.UserId != userId)
                {
                    throw new UnauthorizedAccessException("Você não pode sacar de uma conta que não é sua.");
                }
                    
                if (account.Balance < model.Amount) throw new InvalidOperationException("Saldo insuficiente.");

                account.Balance -= model.Amount;

                var transaction = new Transaction
                {
                    Id = Guid.NewGuid(),
                    Amount = -model.Amount,
                    Date = DateTime.UtcNow,
                    Type = TransactionType.Withdraw,
                    Description = "Saque",
                    FromAccountId = account.AccountID
                };

                var createdTransaction = await _transactionRepository.AddAsync(transaction);
                await _unitOfWork.CommitAsync();

                return TransactionViewModel.FromModel(createdTransaction);
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        public async Task<List<TransactionViewModel>> GetAccountStatementAsync(string accountNumber, DateTime? start, DateTime? end, int pageNumber, int pageSize, string userId)
        {
            var account = await _accountRepository.GetByNumberAsync(accountNumber);
            if (account == null) throw new KeyNotFoundException($"Conta {accountNumber} não encontrada.");

            if (account.Client.UserId != userId)
            {
                throw new UnauthorizedAccessException("Você não tem permissão para ver o extrato desta conta.");
            }

            DateTime? dateStart = start?.Date;
            DateTime? dateEnd = end?.Date.AddDays(1);

            var transactions = await _transactionRepository.GetStatementAsync(account.AccountID, dateStart, dateEnd, pageNumber, pageSize);

            return transactions.Select(t => TransactionViewModel.FromModel(t, account.AccountID)).ToList();
        }
    }
}
