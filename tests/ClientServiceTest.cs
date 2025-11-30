using DevTrails___BankProject.DTOs;
using DevTrails___BankProject.Entities;
using DevTrails___BankProject.Enums;
using DevTrails___BankProject.Repositories.Interfaces;
using DevTrails___BankProject.Service;
using DevTrails___BankProject.Service.Interfaces;
using Moq;

namespace DevTrails___BankProject.Tests
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

            _service = new ClientService(
                _clientRepoMock.Object,
                _accountServiceMock.Object,
                _unitOfWorkMock.Object
            );
        }

        [Fact] 
        public async Task CreateClientAsync_ValidData_ShouldCommitTransaction()
        {
            //arrange

            var userId = Guid.NewGuid().ToString();
            var model = new ClientCreateModel
            {
                Name = "Teste Unitario",
                CPF = "12345678900",
                BirthDate = new DateTime(1990, 1, 1),
                AccountType = AccountType.Checking
            };

            _clientRepoMock.Setup(repo => repo.ExistsByCpfAsync(model.CPF)).ReturnsAsync(false);

            _clientRepoMock.Setup(repo => repo.GetByCpfAsync(model.CPF))
                .ReturnsAsync(new Client
                {
                    Name = model.Name,
                    CPF = model.CPF,
                    UserId = userId,
                    Accounts = new List<Account>() 
                });

            // act

            var result = await _service.CreateClientAsync(model, userId);

            // assert

            Assert.NotNull(result);
            Assert.Equal(model.CPF, result.CPF);

            _unitOfWorkMock.Verify(u => u.BeginTransactionAsync(), Times.Once);

            _clientRepoMock.Verify(repo => repo.AddAsync(It.IsAny<Client>()), Times.Once);

            _clientRepoMock.Verify(repo => repo.SaveChangesAsync(), Times.Once);

            _accountServiceMock.Verify(s => s.CreateAccountAsync(It.IsAny<AccountCreateModel>()), Times.Once);

            _unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Once);

            _unitOfWorkMock.Verify(u => u.RollbackAsync(), Times.Never);
        }

        [Fact]
        public async Task CreateClientAsync_WhenCpfExists_ShouldThrowException()
        {
            // arrange

            var userId = "user123";
            var model = new ClientCreateModel { CPF = "12345678900" };

            _clientRepoMock.Setup(repo => repo.ExistsByCpfAsync(model.CPF))
                .ReturnsAsync(true);

            //act + assert

            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CreateClientAsync(model, userId));

            _clientRepoMock.Verify(repo => repo.AddAsync(It.IsAny<Client>()), Times.Never);

            _unitOfWorkMock.Verify(u => u.BeginTransactionAsync(), Times.Never);
        }
    }
}

