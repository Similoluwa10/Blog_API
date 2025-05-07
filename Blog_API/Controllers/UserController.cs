using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Blog_API.Models;
using Blog_API.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace Blog_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        public readonly BlogDbContext _context;
        public UserController(BlogDbContext context) 
        {
            _context = context;
        }

        [HttpPost]
        public async Task<ActionResult> CreateUser([FromBody] UserModel user)
        {
            _context.UserTable.Add(user);
            await _context.SaveChangesAsync();
            return Ok(user);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")] // Example of role-based access - requires additional setup
        public async Task<ActionResult<IEnumerable<UserModel>>> GetAllUsers()
        {
            return await _context.UserTable.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<UserModel>> GetUserById(int id)
        {
            var user = await _context.UserTable.FindAsync(id);

            if (user == null)
            {
                return NotFound("User not found");
            }

            return user;
        }
    }
}
