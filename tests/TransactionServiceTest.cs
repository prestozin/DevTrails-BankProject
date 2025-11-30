using Moq;
using DevTrails___BankProject.Service;
using DevTrails___BankProject.Repositories.Interfaces;
using DevTrails___BankProject.Entities;
using DevTrails___BankProject.Enums;
using DevTrails___BankProject.DTOs;

namespace DevTrails.Tests
{
    public class TransactionServiceTests
    {
        private readonly Mock<ITransactionRepository> _transRepoMock;
        private readonly Mock<IAccountRepository> _accountRepoMock;
        private readonly Mock<IUnitOfWork> _uowMock;
        private readonly TransactionService _service;

        public TransactionServiceTests()
        {
            _transRepoMock = new Mock<ITransactionRepository>();
            _accountRepoMock = new Mock<IAccountRepository>();
            _uowMock = new Mock<IUnitOfWork>();

            _transRepoMock.Setup(r => r.AddAsync(It.IsAny<Transaction>())).ReturnsAsync((Transaction t) => t);

            _service = new TransactionService(_transRepoMock.Object, _accountRepoMock.Object, _uowMock.Object);
        }

        [Fact]
        public async Task DepositAsync_ValidAmount_ShouldIncreaseBalanceAndCommit()
        {
            // Arrange
            var account = new CheckingAccount { Balance = 100, AccountStatus = AccountStatus.Active };
            var model = new DepositInputModel { AccountNumber = "1234", Amount = 50 };

            _accountRepoMock.Setup(r => r.GetByNumberAsync(model.AccountNumber))
                .ReturnsAsync(account);

            // Act
            var result = await _service.DepositAsync(model);

            // Assert
            Assert.Equal(150, account.Balance);
            Assert.NotNull(result);
            _transRepoMock.Verify(r => r.AddAsync(It.IsAny<Transaction>()), Times.Once);
            _uowMock.Verify(u => u.CommitAsync(), Times.Once);
        }

        [Fact]
        public async Task WithdrawAsync_ValidAmount_ShouldDecreaseBalanceAndCommit()
        {
            // Arrange
            var account = new CheckingAccount { Balance = 100, AccountStatus = AccountStatus.Active };
            var model = new WithdrawInputModel { AccountNumber = "1234", Amount = 40 };

            _accountRepoMock.Setup(r => r.GetByNumberAsync(model.AccountNumber))
                .ReturnsAsync(account);

            // Act
            var result = await _service.WithdrawAsync(model);

            // Assert
            Assert.Equal(60, account.Balance);
            Assert.NotNull(result);
            _uowMock.Verify(u => u.CommitAsync(), Times.Once);
        }

        [Fact]
        public async Task WithdrawAsync_InsufficientFunds_ShouldThrowExceptionAndRollback()
        {
            // Arrange
            var account = new CheckingAccount { Balance = 50, AccountStatus = AccountStatus.Active };
            var model = new WithdrawInputModel { AccountNumber = "1234", Amount = 100 }; // Tenta sacar mais que tem

            _accountRepoMock.Setup(r => r.GetByNumberAsync(model.AccountNumber))
                .ReturnsAsync(account);

            // Act + Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.WithdrawAsync(model));

            Assert.Equal(50, account.Balance);

            _uowMock.Verify(u => u.CommitAsync(), Times.Never);
            _uowMock.Verify(u => u.RollbackAsync(), Times.Once);
        }

        [Fact]
        public async Task TransferAsync_ValidData_ShouldTransferAndChargeFee()
        {
            // Arrange
            var origin = new CheckingAccount { AccountID = Guid.NewGuid(), Balance = 1000, AccountStatus = AccountStatus.Active, Number = "Origem" };
            var destiny = new SavingsAccount { AccountID = Guid.NewGuid(), Balance = 0, AccountStatus = AccountStatus.Active, Number = "Destino" };

            var model = new TransferInputModel
            {
                FromAccountNumber = "Origem",
                ToAccountNumber = "Destino",
                Amount = 100
            };

            _accountRepoMock.Setup(r => r.GetByNumberAsync("Origem")).ReturnsAsync(origin);
            _accountRepoMock.Setup(r => r.GetByNumberAsync("Destino")).ReturnsAsync(destiny);

            // Act
            await _service.TransferAsync(model);

            // Assert 
            Assert.Equal(899.50m, origin.Balance); 
            Assert.Equal(100, destiny.Balance);   


            _transRepoMock.Verify(r => r.AddMultipleTransactionsAsync(It.IsAny<Transaction>(), It.IsAny<Transaction>()), Times.Once);
            _uowMock.Verify(u => u.CommitAsync(), Times.Once);
        }
    
        [Fact]
        public async Task GetAccountStatementAsync_ValidAccount_ShouldReturnHistory()
        {
            // Arrange
            var accId = Guid.NewGuid();
            var account = new CheckingAccount { AccountID = accId, Number = "12345" };

            var transactions = new List<Transaction>
            {
                new Transaction { Id = Guid.NewGuid(), Amount = 100, Type = TransactionType.Deposit, Date = DateTime.Now },
                new Transaction { Id = Guid.NewGuid(), Amount = 50, Type = TransactionType.Withdraw, Date = DateTime.Now }
            };

            _accountRepoMock.Setup(r => r.GetByNumberAsync("12345")).ReturnsAsync(account);

            _transRepoMock.Setup(r => r.GetStatementAsync(accId, It.IsAny<DateTime?>(), It.IsAny<DateTime?>(), 1, 10))
                .ReturnsAsync(transactions);

            // Act
            var result = await _service.GetAccountStatementAsync("12345", null, null, 1, 10);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count); 
            Assert.Equal(100, result[0].Amount); 
        }
    }
}