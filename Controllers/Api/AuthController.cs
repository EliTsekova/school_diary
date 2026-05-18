using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using school_diary.Models;
using school_diary.Dtos;

namespace school_diary.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;

        public AuthController(SignInManager<User> signInManager, UserManager<User> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);

            if (user is null)
                return Unauthorized(new { message = "User not found" });

            var result = await _signInManager.PasswordSignInAsync(
                user,
                dto.Password,
                isPersistent: false,
                lockoutOnFailure: false);

            if (!result.Succeeded)
                return Unauthorized(new { message = "Invalid password" });

            var identityRoles = await _userManager.GetRolesAsync(user);

            var role = identityRoles.FirstOrDefault();

            if (string.IsNullOrWhiteSpace(role))
                role = user.Role.ToString();

            return Ok(new
            {
                role = role,
                email = user.Email,
                message = "Login successful"
            });
        }
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return Ok(new { message = "Logout successful" });
        }
    }
}