using DevTrails___BankProject.DTOs;

namespace DevTrails___BankProject.Service.Interfaces
{
    public interface IClientService
    {
        Task<ClientViewModel> CreateClientAsync(ClientCreateModel dto, string userId);
        Task<ClientViewModel?> GetClientByCpfAsync(string cpf, string userId);
    }
}
