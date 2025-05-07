using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Blog_API.Context;
using Blog_API.Models;

namespace Blog_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly BlogDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(BlogDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            // Check if username already exists
            if (await _context.UserTable.AnyAsync(u => u.Username == model.Username))
            {
                return BadRequest("Username already exists");
            }

            // Check if email already exists
            if (await _context.UserTable.AnyAsync(u => u.Email == model.Email))
            {
                return BadRequest("Email already exists");
            }

            // Create user
            var user = new UserModel
            {
                Name = model.Name,
                Username = model.Username,
                Email = model.Email,
                Password = model.Password // In production, hash the password
            };

            _context.UserTable.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "User registered successfully" });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            var user = await _context.UserTable.SingleOrDefaultAsync(u => 
                u.Username == model.Username && u.Password == model.Password);

            if (user == null)
            {
                return Unauthorized("Invalid username or password");
            }

            var token = GenerateJwtToken(user);
            
            return Ok(new
            {
                token,
                user = new
                {
                    id = user.Id,
                    name = user.Name,
                    username = user.Username,
                    email = user.Email
                }
            });
        }

        private string GenerateJwtToken(UserModel user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Username),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim("id", user.Id.ToString())
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["JwtSettings:Issuer"],
                audience: _configuration["JwtSettings:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(double.Parse(_configuration["JwtSettings:DurationInMinutes"])),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
} 