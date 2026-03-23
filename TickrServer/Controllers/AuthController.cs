using Core.DTOs.Requests;
using Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TickrServer.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController(IRegisterService registerService, ILoginService loginService) : ControllerBase
    {
        private readonly IRegisterService _registerService = registerService;
        private readonly ILoginService _loginService = loginService;

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            var result = await _registerService.RegisterLocalAsync(request);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            
            return Ok(result);
        }

        [HttpPost("google")]
        public async Task<IActionResult> RegisterOrLoginGoogle([FromBody] GoogleAuthRequest request)
        {
            var result = await _registerService.RegisterOrLoginGoogleAsync(request);
            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var result = await _loginService.LoginWithEmailAsync(request);
            if (!result.Success)
            {
                return BadRequest(result);
            }
           
            return Ok(result);
        }

        // test method
        [Authorize]
        [HttpGet("me")]
        public IActionResult Me()
        {
            var userId = User.FindFirst("sub")?.Value;
            var email = User.FindFirst("email")?.Value;
            var tokenType = User.FindFirst("token_type")?.Value;

            return Ok(new { userId, email, tokenType });
        }
    }
}
