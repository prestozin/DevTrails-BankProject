using DevTrails___BankProject.Entities;

namespace DevTrails___BankProject.Service.Interfaces
{
    public interface ITokenService
    {
        string GenerateToken(User user);
    }
}
