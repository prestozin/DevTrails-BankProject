using DevTrails___BankProject.DTOs;
using DevTrails___BankProject.Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DevTrails___BankProject.Controllers
{
    [ApiController]
    [Route("api/transacoes")]
    [Authorize]
    public class TransactionsController : Controller
    {
        private readonly ITransactionService _transactionService;
        public TransactionsController(ITransactionService transactionService)
        {
            _transactionService = transactionService;
        }

        [HttpPost("deposito")]
        public async Task<ActionResult<TransactionViewModel>> Deposit([FromBody] DepositInputModel model)
        {
            var result = await _transactionService.DepositAsync(model);
            return Ok(result);
        }

        [HttpPost("saque")]
        public async Task<ActionResult<TransactionViewModel>> Withdraw([FromBody] WithdrawInputModel model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _transactionService.WithdrawAsync(model, userId);
            return Ok(result);
        }

        [HttpPost("transferencia")]
        public async Task<ActionResult<TransactionViewModel>> Transfer([FromBody] TransferInputModel model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _transactionService.TransferAsync(model, userId);
            return Ok(result);
        }
    }
}
