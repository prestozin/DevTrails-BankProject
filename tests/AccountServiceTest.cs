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
        public async Task CreateAccount_ShouldCreate_WhenDataIsValid()
        {
            var userId = "user1";
            var client = new Client { Id = Guid.NewGuid(), Name = "Teste", CPF = "123", UserId = userId };
            var model = new AccountCreateModel { CPF = "123", AccountType = AccountType.Checking };

            _clientRepoMock.Setup(r => r.GetByCpfAsync("123")).ReturnsAsync(client);
            _accountRepoMock.Setup(r => r.ExistsAccountTypeAsync<CheckingAccount>(client.Id)).ReturnsAsync(false);
            _accountRepoMock.Setup(r => r.ExistsByNumberAsync(It.IsAny<string>())).ReturnsAsync(false);
            _accountRepoMock.Setup(r => r.AddAsync(It.IsAny<Account>())).ReturnsAsync((Account a) => { a.Client = client; return a; });

            var result = await _service.CreateAccountAsync(model, userId);

            Assert.NotNull(result);
            _accountRepoMock.Verify(r => r.AddAsync(It.IsAny<Account>()), Times.Once);
            _accountRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task CreateAccount_ShouldThrow_WhenCheckingAccountAlreadyExists()
        {
            var userId = "user1";
            var client = new Client { Id = Guid.NewGuid(), UserId = userId, Name = "Teste", CPF = "123" };
            var input = new AccountCreateModel { CPF = "123", AccountType = AccountType.Checking };

            _clientRepoMock.Setup(r => r.GetByCpfAsync(input.CPF)).ReturnsAsync(client);
            _accountRepoMock.Setup(r => r.ExistsAccountTypeAsync<CheckingAccount>(client.Id)).ReturnsAsync(true);

            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CreateAccountAsync(input, userId));
        }

        [Fact]
        public async Task CreateAccount_ShouldThrow_WhenUserIsNotTheOwner()
        {
            var userIdLogado = "hacker";
            var donoDoCpf = "dono_real";
            var client = new Client { Id = Guid.NewGuid(), UserId = donoDoCpf, Name = "Vítima", CPF = "123" };
            var input = new AccountCreateModel { CPF = "123", AccountType = AccountType.Savings };

            _clientRepoMock.Setup(r => r.GetByCpfAsync(input.CPF)).ReturnsAsync(client);

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _service.CreateAccountAsync(input, userIdLogado));
        }

        [Fact]
        public async Task GetAccountsByCpf_ShouldReturnList_WhenUserIsAuthorized()
        {
            var userId = "user1";
            var cpf = "123";
            var client = new Client { Id = Guid.NewGuid(), UserId = userId, CPF = cpf };
            var accounts = new List<Account>
            {
                new CheckingAccount { Number = "0001", Balance = 100, Client = client },
                new SavingsAccount { Number = "0002", Balance = 200, Client = client }
            };

            _clientRepoMock.Setup(r => r.GetByCpfAsync(cpf)).ReturnsAsync(client);
            _accountRepoMock.Setup(r => r.GetByClientCpfAsync(cpf)).ReturnsAsync(accounts);

            var result = await _service.GetAccountsByCpfAsync(cpf, userId);

            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task InactivateAccount_ShouldInactivate_WhenBalanceIsZero()
        {
            var userId = "user1";
            var number = "1234";
            var account = new CheckingAccount { Number = number, AccountStatus = AccountStatus.Active, Balance = 0, Client = new Client { UserId = userId } };

            _accountRepoMock.Setup(r => r.GetByNumberAsync(number)).ReturnsAsync(account);

            var result = await _service.InactivateAccountAsync(number, userId);

            Assert.Equal(AccountStatus.Inactive, account.AccountStatus);
            _accountRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task InactivateAccount_ShouldThrow_WhenBalanceIsPositive()
        {
            var userId = "user1";
            var number = "1234";
            var account = new CheckingAccount { Number = number, AccountStatus = AccountStatus.Active, Balance = 100, Client = new Client { UserId = userId } };

            _accountRepoMock.Setup(r => r.GetByNumberAsync(number)).ReturnsAsync(account);

            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.InactivateAccountAsync(number, userId));
        }

        [Fact]
        public async Task ReactivateAccount_ShouldActivate_WhenAccountIsInactive()
        {
            var userId = "user1";
            var number = "1234";
            var account = new CheckingAccount { Number = number, AccountStatus = AccountStatus.Inactive, Client = new Client { UserId = userId } };

            _accountRepoMock.Setup(r => r.GetByNumberAsync(number)).ReturnsAsync(account);

            var result = await _service.ReactivateAccountAsync(number, userId);

            Assert.Equal(AccountStatus.Active, account.AccountStatus);
            _accountRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }
    }
}