using DevTrails___BankProject.Enums;

namespace DevTrails___BankProject.Entities
{
    public class Transaction
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public decimal Amount { get; set; }
        public string? Description { get; set; }
        public DateTime Date { get; set; } = DateTime.UtcNow;
        public TransactionType Type { get; set; }


        public Guid? FromAccountId { get; set; }
        public Account? FromAccount { get; set; }
        public Guid? ToAccountId { get; set; }
        public Account? ToAccount { get; set; }
    }
}
