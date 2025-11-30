using DevTrails___BankProject.Entities;

namespace DevTrails___BankProject.DTOs
{
    public class ClientViewModel
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public string? CPF { get; set; }
        public DateTime BirthDate { get; set; }
        public List<AccountViewModel> Accounts { get; set; } = new();
        public static ClientViewModel FromModel(Client client)
        {
            return new ClientViewModel
            {
                Id = client.Id,
                Name = client.Name,
                CPF = client.CPF,
                BirthDate = client.BirthDate,
                Accounts = client.Accounts.Select(acc =>
                {
                    acc.Client = client;
                    acc.ClientId = client.Id;
                    return AccountViewModel.FromModel(acc);
                }).ToList()
            };
        }
    }
}
