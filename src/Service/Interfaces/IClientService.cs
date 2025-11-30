using DevTrails___BankProject.DTOs;
using DevTrails___BankProject.Entities;

namespace DevTrails___BankProject.Service.Interfaces
{
    public interface IClientService
    {
        Task<ClientViewModel> CreateClientAsync(ClientCreateModel dto, string userId);
        Task<ClientViewModel?> GetClientByCpfAsync(string cpf);
    }
}
