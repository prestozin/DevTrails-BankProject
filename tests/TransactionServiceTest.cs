using Moq;
using DevTrails___BankProject.Service;
using DevTrails___BankProject.Repositories.Interfaces;
using DevTrails___BankProject.Entities;
using DevTrails___BankProject.Enums;
using DevTrails___BankProject.DTOs;
using Microsoft.EntityFrameworkCore;

namespace DevTrails.Tests
{
    public class TransactionServiceTests
    {
        private readonly Mock<ITransactionRepository> _transactionRepoMock;
        private readonly Mock<IAccountRepository> _accountRepoMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly TransactionService _service;

        public TransactionServiceTests()
        {
            _transactionRepoMock = new Mock<ITransactionRepository>();
            _accountRepoMock = new Mock<IAccountRepository>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();

            _transactionRepoMock.Setup(r => r.AddAsync(It.IsAny<Transaction>())).ReturnsAsync((Transaction t) => t);
            _service = new TransactionService(_transactionRepoMock.Object, _accountRepoMock.Object, _unitOfWorkMock.Object);
        }

        [Fact]
        public async Task Deposit_ShouldIncreaseBalance_WhenAmountIsValid()
        {
            var account = new CheckingAccount { Balance = 100, AccountStatus = AccountStatus.Active };
            var model = new DepositInputModel { AccountNumber = "1234", Amount = 50 };

            _accountRepoMock.Setup(r => r.GetByNumberAsync(model.AccountNumber)).ReturnsAsync(account);

            await _service.DepositAsync(model);

            Assert.Equal(150, account.Balance);
            _unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Once);
        }

        [Fact]
        public async Task Withdraw_ShouldDecreaseBalance_WhenBalanceIsSufficient()
        {
            var userId = "user1";
            var account = new CheckingAccount { Balance = 100, AccountStatus = AccountStatus.Active, Client = new Client { UserId = userId } };
            var model = new WithdrawInputModel { AccountNumber = "1234", Amount = 40 };

            _accountRepoMock.Setup(r => r.GetByNumberAsync(model.AccountNumber)).ReturnsAsync(account);

            await _service.WithdrawAsync(model, userId);

            Assert.Equal(60, account.Balance);
            _unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Once);
        }

        [Fact]
        public async Task Withdraw_ShouldThrow_WhenBalanceIsInsufficient()
        {
            var userId = "user1";
            var account = new CheckingAccount { Balance = 50, AccountStatus = AccountStatus.Active, Client = new Client { UserId = userId } };
            var model = new WithdrawInputModel { AccountNumber = "1234", Amount = 100 };

            _accountRepoMock.Setup(r => r.GetByNumberAsync(model.AccountNumber)).ReturnsAsync(account);

            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.WithdrawAsync(model, userId));
            _unitOfWorkMock.Verify(u => u.RollbackAsync(), Times.Once);
        }

        [Fact]
        public async Task Transfer_ShouldTransferAndChargeFee_WhenDataIsValid()
        {
            var userId = "user1";
            var origin = new CheckingAccount { AccountID = Guid.NewGuid(), Balance = 1000, AccountStatus = AccountStatus.Active, Number = "Origem", Client = new Client { UserId = userId } };
            var destiny = new SavingsAccount { AccountID = Guid.NewGuid(), Balance = 0, AccountStatus = AccountStatus.Active, Number = "Destino", Client = new Client { UserId = "user2" } };
            var model = new TransferInputModel { FromAccountNumber = "Origem", ToAccountNumber = "Destino", Amount = 100 };

            _accountRepoMock.Setup(r => r.GetByNumberAsync("Origem")).ReturnsAsync(origin);
            _accountRepoMock.Setup(r => r.GetByNumberAsync("Destino")).ReturnsAsync(destiny);

            await _service.TransferAsync(model, userId);

            Assert.Equal(899.50m, origin.Balance);
            Assert.Equal(100, destiny.Balance);
            _unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Once);
        }

        [Fact]
        public async Task GetStatement_ShouldReturnTransactions_WhenCalled()
        {
            var userId = "user1";
            var accId = Guid.NewGuid();
            var account = new CheckingAccount { AccountID = accId, Number = "12345", Client = new Client { UserId = userId } };
            var transactions = new List<Transaction> { new Transaction { Amount = 100 } };

            _accountRepoMock.Setup(r => r.GetByNumberAsync("12345")).ReturnsAsync(account);
            _transactionRepoMock.Setup(r => r.GetStatementAsync(accId, It.IsAny<DateTime?>(), It.IsAny<DateTime?>(), 1, 10)).ReturnsAsync(transactions);

            var result = await _service.GetAccountStatementAsync("12345", null, null, 1, 10, userId);

            Assert.NotNull(result);
            Assert.Single(result);
        }

        [Fact]
        public async Task Transfer_ShouldRollback_WhenDatabaseFails()
        {
            var userId = "user1";
            var sender = new CheckingAccount { AccountID = Guid.NewGuid(), Balance = 1000, Client = new Client { UserId = userId }, AccountStatus = AccountStatus.Active };
            var receiver = new CheckingAccount { AccountID = Guid.NewGuid(), Balance = 0, AccountStatus = AccountStatus.Active };
            var input = new TransferInputModel { FromAccountNumber = "1111", ToAccountNumber = "2222", Amount = 100 };

            _accountRepoMock.Setup(r => r.GetByNumberAsync("1111")).ReturnsAsync(sender);
            _accountRepoMock.Setup(r => r.GetByNumberAsync("2222")).ReturnsAsync(receiver);
            _unitOfWorkMock.Setup(u => u.CommitAsync()).ThrowsAsync(new Exception("Erro fatal!"));

            await Assert.ThrowsAsync<Exception>(() => _service.TransferAsync(input, userId));
            _unitOfWorkMock.Verify(u => u.RollbackAsync(), Times.Once);
        }

        [Fact]
        public async Task Withdraw_ShouldThrow_WhenConcurrencyConflictOccurs()
        {
            var userId = "user1";
            var account = new CheckingAccount { AccountID = Guid.NewGuid(), Balance = 100, Client = new Client { UserId = userId }, AccountStatus = AccountStatus.Active, RowVersion = new byte[] { 0, 0, 0, 1 } };
            var input = new WithdrawInputModel { AccountNumber = "1111", Amount = 50 };

            _accountRepoMock.Setup(r => r.GetByNumberAsync("1111")).ReturnsAsync(account);
            _unitOfWorkMock.Setup(u => u.CommitAsync()).ThrowsAsync(new DbUpdateConcurrencyException("Conflito", new List<Microsoft.EntityFrameworkCore.Update.IUpdateEntry>()));

            await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => _service.WithdrawAsync(input, userId));
            _unitOfWorkMock.Verify(u => u.RollbackAsync(), Times.Once);
        }
    }
}