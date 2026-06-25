using Medicine.DTOs;
using Medicine.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Medicine.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IConfiguration configuration,
            ILogger<AuthController> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
            _logger = logger;
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            if (!ModelState.IsValid)
            {
                _logger?.LogWarning("Register failed: invalid model state");
                return BadRequest(ModelState);
            }

            _logger?.LogInformation("Register attempt for {Email} with role {Role}", dto.Email, dto.Role);
            var userExists = await _userManager.FindByEmailAsync(dto.Email);
            if (userExists != null)
                return BadRequest("Email đã được sử dụng.");

            ApplicationUser user = new ApplicationUser()
            {
                Email = dto.Email,
                SecurityStamp = Guid.NewGuid().ToString(),
                UserName = dto.Email,
                FullName = dto.FullName
            };
            
            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description).ToList();
                _logger?.LogWarning("Register failed for {Email}: {Errors}", dto.Email, string.Join("; ", errors));
                return BadRequest(new { message = "Tạo tài khoản thất bại", errors });
            }

            if (!await _roleManager.RoleExistsAsync(dto.Role))
            {
                await _roleManager.CreateAsync(new IdentityRole(dto.Role));
            }
            if (await _roleManager.RoleExistsAsync(dto.Role))
            {
                await _userManager.AddToRoleAsync(user, dto.Role);
            }

            return Ok("Tạo tài khoản thành công!");
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            if (!ModelState.IsValid)
            {
                _logger?.LogWarning("Login failed: invalid model state");
                return BadRequest(ModelState);
            }
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null)
            {
                _logger?.LogWarning("Login failed: user not found for email {Email}", dto.Email);
                return Unauthorized("Đăng nhập thất bại. Sai email hoặc mật khẩu.");
            }

            var passwordValid = await _userManager.CheckPasswordAsync(user, dto.Password);
            if (!passwordValid)
            {
                _logger?.LogWarning("Login failed: incorrect password for email {Email}", dto.Email);
                return Unauthorized("Đăng nhập thất bại. Sai email hoặc mật khẩu.");
            }

            // user exists and password is valid -> generate response
            return await GenerateLoginResponse(user);
        }

        // Backwards-compatible routes: some frontends may call other paths like /api/auth, /api/authenticate or /api/auth/signin
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> LoginRoot([FromBody] LoginDto dto)
        {
            return await Login(dto);
        }

        [HttpPost("authenticate")]
        [AllowAnonymous]
        public async Task<IActionResult> Authenticate([FromBody] LoginDto dto)
        {
            return await Login(dto);
        }

        [HttpPost("signin")]
        [AllowAnonymous]
        public async Task<IActionResult> SignIn([FromBody] LoginDto dto)
        {
            return await Login(dto);
        }

        private async Task<IActionResult> GenerateLoginResponse(ApplicationUser user)
        {
            var userRoles = await _userManager.GetRolesAsync(user);

            var authClaims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName ?? ""),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };

            foreach (var userRole in userRoles)
            {
                authClaims.Add(new Claim(ClaimTypes.Role, userRole));
                authClaims.Add(new Claim("role", userRole));
            }

            if (userRoles.Any())
            {
                authClaims.Add(new Claim("roles", string.Join(',', userRoles)));
            }

            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? "superSecretKey@3456789012345678901234567890"));

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"] ?? "http://localhost:7149",
                audience: _configuration["Jwt:Audience"] ?? "MedicineApi",
                expires: DateTime.Now.AddHours(3),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            // Return token and roles in response (camelCase due to JSON options)
            return Ok(new AuthResponseDto
            {
                Token = tokenString,
                Email = user.Email ?? "",
                FullName = user.FullName,
                Roles = userRoles.ToList()
            });
        }
    }
}
