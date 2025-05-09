using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Blog_API.Models;
using Blog_API.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace Blog_API.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        public readonly BlogDbContext _context;
        public UserController(BlogDbContext context) 
        {
            _context = context;
        }

        [HttpGet]
        [Authorize(Roles = "User")]
        public async Task<IEnumerable<UserModel>> GetAllUsers()
        {
            return await _context.UserTable.ToListAsync();
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "User")]
        public async Task<ActionResult<UserModel>> GetUserById(int id)
        {
            return await _context.UserTable.FirstOrDefaultAsync(t => t.Id == id);
        }

    }
}
