namespace DevTrails___BankProject.Entities
{
    public class Client
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string? Name { get; set; } 
        public string? CPF {  get; set; }
        public DateTime BirthDate { get; set; }
        public ICollection<Account> Accounts { get; set; } = new List<Account>();

        public string? UserId { get; set; } 
        public User? User { get; set; }
    }
}
