using DevTrails___BankProject.DTOs;
using DevTrails___BankProject.Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DevTrails___BankProject.Controllers
{
    [ApiController]
    [Route("api/contas")]
    [Authorize]
    public class AccountsController : Controller
    {
        private readonly IAccountService _accountService;
        private readonly ITransactionService _transactionService;
        public AccountsController(IAccountService accountService, ITransactionService transactionService)
        {
            _accountService = accountService;
            _transactionService = transactionService;
        }

        [HttpPost]
        public async Task<ActionResult<AccountViewModel>> CreateAccount([FromBody] AccountCreateModel model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _accountService.CreateAccountAsync(model, userId);
            return Created($"/api/contas/{result.AccountId}", result);
        }

        [HttpGet("cliente/{cpf}")]
        public async Task<ActionResult<List<AccountViewModel>>> GetAccountsByClient(string cpf)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _accountService.GetAccountsByCpfAsync(cpf, userId);
            return Ok(result);
        }

        [HttpGet("{accountNumber}/extrato")]
        public async Task<ActionResult<List<TransactionViewModel>>> GetAccountStatement
            (string accountNumber,
            [FromQuery] DateTime? start,
            [FromQuery] DateTime? end,
            [FromQuery] int pageNumber = 1, 
            [FromQuery] int pageSize = 10)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _transactionService.GetAccountStatementAsync(accountNumber, start, end, pageNumber, pageSize, userId);
            return Ok(result);
        }

        [HttpDelete("{accountNumber}/inativar")]
        public async Task <IActionResult> CloseAccount(string accountNumber)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            await _accountService.InactivateAccountAsync(accountNumber, userId);
            return NoContent();
        }

        [HttpPatch("{accountNumber}/ativar")]
        public async Task<ActionResult<AccountViewModel>> ReopenAccount(string accountNumber)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _accountService.ReactivateAccountAsync(accountNumber, userId);
            return Ok(result);
        }
    }
}
