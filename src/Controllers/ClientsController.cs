using DevTrails___BankProject.DTOs;
using DevTrails___BankProject.Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DevTrails___BankProject.Controllers
{
    [ApiController]
    [Route("api/clientes")]
    [Authorize]
    public class ClientsController : Controller
    {
       private readonly IClientService _clientService;

        public ClientsController(IClientService clientService)
        {
            _clientService = clientService;
        }

        [HttpPost]
        public async Task<ActionResult<ClientViewModel>> CreateClient([FromBody] ClientCreateModel model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId)) return Unauthorized("Token inválido ou sem ID de usuário.");
       
            var newClient = await _clientService.CreateClientAsync(model, userId);
            return CreatedAtAction(nameof(GetByCpf), new { cpf = newClient.CPF }, newClient);
        }

        [HttpGet("cpf/{cpf}")]
        public async Task<ActionResult<ClientViewModel>> GetByCpf(string cpf)
        {
            var client = await _clientService.GetClientByCpfAsync(cpf);
            if (client == null) return NotFound("Cliente não encontrado.");
            return Ok(client);
        }
    }
}
