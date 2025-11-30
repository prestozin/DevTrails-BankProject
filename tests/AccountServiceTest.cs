using Moq;
using DevTrails___BankProject.Service;
using DevTrails___BankProject.Repositories.Interfaces;
using DevTrails___BankProject.DTOs;
using DevTrails___BankProject.Entities;
using DevTrails___BankProject.Enums;

namespace DevTrails.Tests
{
    public class AccountServiceTests
    {
        private readonly Mock<IAccountRepository> _accountRepoMock;
        private readonly Mock<IClientRepository> _clientRepoMock;

        private readonly AccountService _service;

        public AccountServiceTests()
        {
            _accountRepoMock = new Mock<IAccountRepository>();
            _clientRepoMock = new Mock<IClientRepository>();

            _service = new AccountService(_accountRepoMock.Object, _clientRepoMock.Object);
        }

        [Fact]
        public async Task CreateAccountAsync_ValidData_ShouldCreateAndSave()
        {
            // Arrange
            var client = new Client { Id = Guid.NewGuid(), Name = "Teste", CPF = "123" };
            var model = new AccountCreateModel { CPF = "123", AccountType = AccountType.Checking };

            _clientRepoMock.Setup(r => r.GetByCpfAsync("123"))
                .ReturnsAsync(client);

            _accountRepoMock.Setup(r => r.ExistsAccountTypeAsync<CheckingAccount>(client.Id))
                .ReturnsAsync(false);

            _accountRepoMock.Setup(r => r.ExistsByNumberAsync(It.IsAny<string>()))
                .ReturnsAsync(false);

            _accountRepoMock.Setup(r => r.AddAsync(It.IsAny<Account>()))
                .ReturnsAsync((Account a) => a); 

            // Act
            var result = await _service.CreateAccountAsync(model);

            // Assert
            Assert.NotNull(result);

            _accountRepoMock.Verify(r => r.AddAsync(It.IsAny<Account>()), Times.Once);
            _accountRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task GetAccountsByCpfAsync_ShouldReturnList()
        {
            // Arrange
            var cpf = "123";
            var accounts = new List<Account>
            {
                new CheckingAccount { Number = "0001", Balance = 100 },
                new SavingsAccount {  Number = "0002", Balance = 200 }
            };

            _accountRepoMock.Setup(r => r.GetByClientCpfAsync(cpf))
                .ReturnsAsync(accounts);

            // Act
            var result = await _service.GetAccountsByCpfAsync(cpf);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task InactivateAccountAsync_ActiveAccountZeroBalance_ShouldInactivateAndSave()
        {
            // Arrange
            var number = "1234";
   
            var account = new CheckingAccount { Number = number, AccountStatus = AccountStatus.Active, Balance = 0 };

            _accountRepoMock.Setup(r => r.GetByNumberAsync(number)).ReturnsAsync(account);

            // Act
            await _service.InactivateAccountAsync(number);

            // Assert
            Assert.Equal(AccountStatus.Inactive, account.AccountStatus); 

            _accountRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task InactivateAccountAsync_BalanceGreaterThanZero_ShouldThrowException()
        {
            // Arrange
            var number = "1234";
            var account = new CheckingAccount { Number = number, AccountStatus = AccountStatus.Active, Balance = 100 }; 

            _accountRepoMock.Setup(r => r.GetByNumberAsync(number)).ReturnsAsync(account);

            // Act + Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.InactivateAccountAsync(number));
            _accountRepoMock.Verify(r => r.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task ReactivateAccountAsync_InactiveAccount_ShouldActivateAndSave()
        {
            // Arrange
            var number = "1234";
            var account = new CheckingAccount { Number = number, AccountStatus = AccountStatus.Inactive };

            _accountRepoMock.Setup(r => r.GetByNumberAsync(number)).ReturnsAsync(account);

            // Act
            var result = await _service.ReactivateAccountAsync(number);

            // Assert
            Assert.Equal(AccountStatus.Active, account.AccountStatus);
            Assert.NotNull(result);
            _accountRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }
    }
}