using DevTrails___BankProject.Entities;

namespace DevTrails___BankProject.Repositories.Interfaces
{
    public interface IClientRepository : IBaseRepository<Client>
    {
        Task<Client?> GetByCpfAsync(string cpf);
        Task<bool> ExistsByCpfAsync(string cpf);
    }
}
