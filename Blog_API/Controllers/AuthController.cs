using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Blog_API.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Blog_API.Context;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace Blog_API.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly BlogDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(BlogDbContext context, IConfiguration configuration)
        {
            _configuration = configuration;
            _context = context;
        }

        
        [HttpPost]
        public async Task<ActionResult> Register([FromBody] RegisterModel newUser )
        {
            if (newUser == null)
            {
                return BadRequest("User Credentials not specified!");
            }

            var newUserModel = new UserModel
            {
                Name = newUser.Name,
                Email = newUser.Email,
                Username = newUser.Username,
                Password = newUser.Password,
            };
           
            _context.UserTable.Add(newUserModel);
            await _context.SaveChangesAsync();

            return Ok("User Successfully created");
        }

        [HttpPost]
        public async Task<ActionResult> Login(LoginModel userLogin)
        {
            if (string.IsNullOrEmpty(userLogin.Email) || string.IsNullOrEmpty(userLogin.Password))
                return BadRequest("Please input Email and Password");

            var user = await _context.UserTable.FirstOrDefaultAsync(t => t.Email == userLogin.Email && t.Password == userLogin.Password);

            if (user == null)
                return BadRequest("Invalid Email or Password");

            var token =  GenerateJwtTokenForUser(user);

            return Ok(new
            {
                token,
                user = new
                {
                    id = user.Id,
                    name = user.Name,
                    username = user.Username,
                    email = user.Email,
                }
            }
           );

        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult> Logout()
        {
            var jti = User.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;
            var expClaim = User.FindFirst(JwtRegisteredClaimNames.Exp)?.Value;

            //check if token is null
            if (string.IsNullOrEmpty(jti))
                return BadRequest("You are not currently logged in!! Invalid tokens");

            //check if token is expired
            DateTime? expiry = null;
            if (long.TryParse(expClaim, out var expUnix))
            {
                expiry = DateTimeOffset.FromUnixTimeSeconds(expUnix).UtcDateTime;
            }

            //add to blacklistedtokens database
            var blackListedToken = new BlackListTokensModel
            {
                Jti = jti,
                DateTime =  expiry ?? DateTime.UtcNow.AddMinutes(
                Convert.ToDouble(_configuration["JwtSettings:DurationInMinutes"])
                )
            };

            await _context.BlackListToken.AddAsync(blackListedToken);
            return Ok("Logout successful");
        }           



        private string GenerateJwtTokenForUser(UserModel user)
        {           
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
            new Claim(JwtRegisteredClaimNames.Sub, user.Username),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.Role, "User"),
            new Claim("id", user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

           
            var token = new JwtSecurityToken(
                issuer: _configuration["JwtSettings:Issuer"],
                audience: _configuration["JwtSettings:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuration["JwtSettings:DurationInMinutes"])),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }

}

