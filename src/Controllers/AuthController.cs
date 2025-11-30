using DevTrails___BankProject.DTOs.InputModel;
using DevTrails___BankProject.Entities;
using DevTrails___BankProject.Service.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace DevTrails___BankProject.Controllers
{
    public class AuthController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly ITokenService _tokenService;

        public AuthController(UserManager<User> userManager, SignInManager<User> signInManager, ITokenService tokenService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenService = tokenService;
        }

        [HttpPost("registrar")]
        public async Task<ActionResult> Register(RegisterInputModel model)
        {
            if (model.Password != model.ConfirmPassword)
                return BadRequest("As senhas não conferem.");

            var user = new User
            {
                UserName = model.Email, 
                Email = model.Email
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            return Ok(new { message = "Usuário criado com sucesso!" });
        }

        [HttpPost("login")]
        public async Task<ActionResult> Login(LoginInputModel model)
        {
            var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, false, false);

            if (!result.Succeeded)
            {
                return Unauthorized("Usuário ou senha inválidos.");
            }


            var user = await _userManager.FindByEmailAsync(model.Email);
            var token = _tokenService.GenerateToken(user);

            return Ok(new
            {
                email = user.Email,
                token = token
            });
        }
    }
}
