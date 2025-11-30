using DevTrails___BankProject.Entities;
using DevTrails___BankProject.Enums;

namespace DevTrails___BankProject.DTOs
{
    public class TransactionViewModel
    {
        public Guid Id { get; set; }
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
        public string? Type { get; set; }
        public string? Description { get; set; }

        public static TransactionViewModel FromModel(Transaction transaction, Guid? viewerAccountId = null)
        {
            

            return new TransactionViewModel
            {
                Id = transaction.Id,
                Amount = transaction.Amount,
                Date = transaction.Date,
                Type = transaction.Type.ToString(),
                Description = transaction.Description
            };
        }
    }
}
