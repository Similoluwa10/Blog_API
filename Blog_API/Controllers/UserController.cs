using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Blog_API.Models;
using Blog_API.Context;

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

        //[HttpGet]
        //public async Task<IEnumerable> GetAllUsers()
        //{
        //    await return _context.UserTable.ToList();
        //}

    }
}
