using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using YTM.Core.Entities;
using YTM.Core.Services;
using BCrypt.Net;

namespace YTM.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IUserService userService, IConfiguration configuration, ILogger<AuthController> logger)
        {
            _userService = userService;
            _configuration = configuration;
            _logger = logger;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            try
            {
                _logger.LogInformation($"Register attempt for email: {model.Email}");

                if (await _userService.EmailExistsAsync(model.Email))
                {
                    _logger.LogWarning($"Registration failed: Email already exists - {model.Email}");
                    return BadRequest(new { message = "Bu e-posta adresi zaten kayıtlı!" });
                }

                var user = new User
                {
                    Email = model.Email,
                    Password = BCrypt.Net.BCrypt.HashPassword(model.Password),
                    Role = "Customer"
                };

                await _userService.CreateUserAsync(user);
                _logger.LogInformation($"User registered successfully: {model.Email}");

                return Ok(new { message = "Kayıt başarılı!" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Registration error: {ex.Message}");
                return StatusCode(500, new { message = "Kayıt işlemi sırasında bir hata oluştu." });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            try
            {
                _logger.LogInformation($"Login attempt for email: {model.Email}");

                var user = await _userService.GetUserByEmailAsync(model.Email);

                if (user == null || !BCrypt.Net.BCrypt.Verify(model.Password, user.Password))
                {
                    _logger.LogWarning($"Login failed: Invalid credentials for {model.Email}");
                    return Unauthorized(new { message = "Geçersiz e-posta veya şifre!" });
                }

                var token = GenerateJwtToken(user);
                _logger.LogInformation($"Login successful for {model.Email}");

                return Ok(new { token, role = user.Role });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Login error: {ex.Message}");
                return StatusCode(500, new { message = "Giriş işlemi sırasında bir hata oluştu." });
            }
        }

        private string GenerateJwtToken(User user)
        {
            if (user.Id == null)
            {
                throw new ArgumentException("User ID cannot be null");
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? throw new InvalidOperationException()));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim("UserId", user.Id)
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }

    public class LoginModel
    {
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
    }

    public class RegisterModel
    {
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
    }
} 