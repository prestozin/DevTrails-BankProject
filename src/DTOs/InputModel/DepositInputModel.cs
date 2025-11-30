using System.ComponentModel.DataAnnotations;

namespace DevTrails___BankProject.DTOs
{
    public class DepositInputModel
    {
        public string? AccountNumber { get; set; }
        public decimal Amount { get; set; }
    }
}
