namespace DevTrails___BankProject.DTOs
{
    public class TransferInputModel
    {
        public string? FromAccountNumber { get; set; }
        public string? ToAccountNumber { get; set; }
        public decimal Amount { get; set; }
    }
}
