using DevTrails___BankProject.Enums;
using System.ComponentModel.DataAnnotations;

namespace DevTrails___BankProject.Entities
{
    public abstract class Account
    {
        public Guid AccountID { get; set; } = Guid.NewGuid();
        public string? Number { get; set; } 
        public decimal Balance { get; set; }
        public DateTime CreationDate { get; set; }
        public AccountStatus AccountStatus { get; set; }


        public Guid ClientId { get; set; }
        public Client? Client { get; set; }
        public ICollection<Transaction> SentTransactions { get; set; } = new List<Transaction>();
        public ICollection<Transaction> ReceivedTransactions { get; set; } = new List<Transaction>();

        [Timestamp]
        public byte[]? RowVersion { get; set; }
    }
}
