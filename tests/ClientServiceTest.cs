using Moq;
using DevTrails___BankProject.Service;
using DevTrails___BankProject.Service.Interfaces;
using DevTrails___BankProject.Repositories.Interfaces;
using DevTrails___BankProject.DTOs;
using DevTrails___BankProject.Entities;
using DevTrails___BankProject.Enums;

namespace DevTrails.Tests
{
    public class ClientServiceTest
    {
        private readonly Mock<IClientRepository> _clientRepoMock;
        private readonly Mock<IAccountService> _accountServiceMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly ClientService _service;

        public ClientServiceTest()
        {
            _clientRepoMock = new Mock<IClientRepository>();
            _accountServiceMock = new Mock<IAccountService>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _service = new ClientService(_clientRepoMock.Object, _accountServiceMock.Object, _unitOfWorkMock.Object);
        }

        [Fact]
        public async Task CreateClient_ShouldCommitTransaction_WhenDataIsValid()
        {
            var userId = Guid.NewGuid().ToString();
            var model = new ClientCreateModel { Name = "Teste", CPF = "12345678900", BirthDate = new DateTime(1990, 1, 1), AccountType = AccountType.Checking };

            _clientRepoMock.Setup(repo => repo.ExistsByCpfAsync(model.CPF)).ReturnsAsync(false);
            _clientRepoMock.Setup(repo => repo.GetByCpfAsync(model.CPF)).ReturnsAsync(new Client { Name = model.Name, CPF = model.CPF, UserId = userId, Accounts = new List<Account>() });

            await _service.CreateClientAsync(model, userId);

            _unitOfWorkMock.Verify(u => u.BeginTransactionAsync(), Times.Once);
            _clientRepoMock.Verify(repo => repo.AddAsync(It.IsAny<Client>()), Times.Once);
            _accountServiceMock.Verify(s => s.CreateAccountAsync(It.IsAny<AccountCreateModel>(), userId), Times.Once);
            _unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Once);
        }

        [Fact]
        public async Task CreateClient_ShouldThrow_WhenCpfAlreadyExists()
        {
            var userId = "user123";
            var model = new ClientCreateModel { CPF = "12345678900" };
            _clientRepoMock.Setup(repo => repo.ExistsByCpfAsync(model.CPF)).ReturnsAsync(true);

            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CreateClientAsync(model, userId));
            _clientRepoMock.Verify(repo => repo.AddAsync(It.IsAny<Client>()), Times.Never);
        }
    }
}