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
            string formattedDescription = transaction.Description;

            if (viewerAccountId.HasValue && transaction.Type == TransactionType.Transfer)
            {

                if (transaction.FromAccountId == viewerAccountId)
                {
                    var destinationNumber = transaction.ToAccount?.Number ?? "Desconhecido";
                    formattedDescription = $"Transferência enviada para: {destinationNumber}";
                }
                else if (transaction.ToAccountId == viewerAccountId)
                {
                    var originNumber = transaction.FromAccount?.Number ?? "Desconhecido";
                    formattedDescription = $"Transferência recebida de: {originNumber}";
                }
            }
            if (transaction.Type == TransactionType.ServiceFee)
            {
                formattedDescription = "Tarifa bancária";
            }

            return new TransactionViewModel
            {
                Id = transaction.Id,
                Amount = transaction.Amount,
                Date = transaction.Date,
                Type = transaction.Type.ToString(),
                Description = formattedDescription
            };
        }
    }
}
