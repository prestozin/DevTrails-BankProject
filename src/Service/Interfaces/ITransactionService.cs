using DevTrails___BankProject.DTOs;
using DevTrails___BankProject.Entities;

namespace DevTrails___BankProject.Service.Interfaces
{
    public interface ITransactionService
    {
        Task<TransactionViewModel> DepositAsync(DepositInputModel model);
        Task<TransactionViewModel> WithdrawAsync(WithdrawInputModel model, string userId);
        Task<TransactionViewModel> TransferAsync(TransferInputModel model, string userId);
        Task<List<TransactionViewModel>> GetAccountStatementAsync(string accountNumber, DateTime? start, DateTime? end, int pageNumber, int pageSize, string userId);
    }
}
