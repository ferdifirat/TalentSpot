using TalentSpot.Application.DTOs;
using TalentSpot.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace TalentSpot.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost]
        public async Task<IActionResult> Register([FromBody] UserCreateDTO userCreateDTO)
        {
            var result = await _userService.RegisterUserWithCompanyAsync(userCreateDTO);
            return Ok(result);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO request)
        {
            try
            {
                var token = await _userService.LoginAsync(request.PhoneNumber, request.Password);
                return Ok(new { Token = token });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { Message = ex.Message });
            }
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromHeader] string authorization)
        {
            var token = authorization?.Replace("Bearer ", string.Empty);
            if (string.IsNullOrEmpty(token))
            {
                return BadRequest(new { Message = "Invalid token" });
            }

            await _userService.LogoutAsync(token);
            return Ok(new { Message = "Logout successful" });
        }
    }
}