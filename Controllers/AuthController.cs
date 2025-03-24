using BugTrackingSystem.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.Threading.Tasks;
using BugTrackingSystem.Dto;

namespace BugTrackingSystem.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public AuthController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto model)
        {
            // Manually check if Password and ConfirmPassword match
            if (model.Password != model.ConfirmPassword)
            {
                return BadRequest(new { message = "Passwords do not match" });
            }

            if (!ModelState.IsValid)
            {
                // Return detailed validation errors from ModelState
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(new { errors });
            }

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = model.FullName
            };

            // Create the user using the _userManager service
            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                // Return errors if user creation failed
                var errors = result.Errors.Select(e => e.Description);
                return BadRequest(new { errors });
            }

            // Optionally log the user in after registration
            await _signInManager.SignInAsync(user, isPersistent: false);

            return Ok(new { message = "User registered and logged in successfully" });
        }


        // Login a user
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null) return Unauthorized("Invalid credentials");

            var result = await _signInManager.PasswordSignInAsync(user, model.Password,false,false);

            if (!result.Succeeded) return Unauthorized("Invalid credentials");

            return Ok(new { message = "User logged in successfully" });
        }

        // Logout a user
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return Ok(new { message  ="User logged out successfully" });
        }

        [HttpGet("isLoggedIn")]
        public IActionResult IsLoggedIn()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Ok(false); 
            }
            return Ok(true);  
        }

    }
}
