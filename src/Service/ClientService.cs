using DevTrails___BankProject.Data;
using DevTrails___BankProject.Entities;
using DevTrails___BankProject.Service.Interfaces;
using DevTrails___BankProject.Repositories.Interfaces;
using DevTrails___BankProject.DTOs;

namespace DevTrails___BankProject.Service
{
    public class ClientService : IClientService
    {
        private readonly IClientRepository _clientRepository;
        private readonly IAccountService _accountService;
        private readonly IUnitOfWork _unitOfWork;

        public ClientService(IClientRepository clientRepository, IAccountService accountService, IUnitOfWork unitOfWork)
        {
            _clientRepository = clientRepository;
            _accountService = accountService;
            _unitOfWork = unitOfWork;

        }
        public async Task<ClientViewModel> CreateClientAsync(ClientCreateModel model, string userId)
        {
            if (await _clientRepository.ExistsByCpfAsync(model.CPF)) throw new InvalidOperationException("CPF já cadastrado.");
     
            await _unitOfWork.BeginTransactionAsync();

            try
            {
                var client = new Client
                {
                    Id = Guid.NewGuid(),
                    Name = model.Name,
                    CPF = model.CPF,
                    BirthDate = model.BirthDate,
                    UserId = userId
                };

                await _clientRepository.AddAsync(client);
                await _clientRepository.SaveChangesAsync();

                var account = new AccountCreateModel
                {
                    CPF = model.CPF,
                    AccountType = model.AccountType
                };

                await _accountService.CreateAccountAsync(account);
                await _unitOfWork.CommitAsync();

                var resultClient = await _clientRepository.GetByCpfAsync(model.CPF);
                return ClientViewModel.FromModel(resultClient);
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        public async Task<ClientViewModel?> GetClientByCpfAsync(string cpf)
        {
            var client = await _clientRepository.GetByCpfAsync(cpf);
            if (client == null) return null;

            return ClientViewModel.FromModel(client);
        }
    }
}
