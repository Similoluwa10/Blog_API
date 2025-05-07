using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Blog_API.Context;
using Blog_API.Models;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Blog_API.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class BlogPostController : ControllerBase
    {
        private readonly BlogDbContext _context;        
        public BlogPostController(BlogDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        [Authorize] // Require authentication
        public async Task<ActionResult> CreateBlog([FromBody] BlogPostModel newBlog)
        {
            // Get the authenticated user ID
            var userId = GetUserId();
            if (userId == 0)
                return Unauthorized("Invalid user token");

            // Set the blog's author to the authenticated user
            newBlog.UserId = userId;

            _context.BlogPostTable.Add(newBlog);
            await _context.SaveChangesAsync();
            return Ok(newBlog);
        }

        [HttpGet]
        public async Task<IEnumerable<BlogPostModel>> GetAllBlogs()
        {
            return await _context.BlogPostTable.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetBlogById(int id)
        {
            var blog = await _context.BlogPostTable.FirstOrDefaultAsync(t => t.Id == id);

            if (blog == null)
            {
                return NotFound("Item does not exist");
            }
            return Ok(blog);
        }

        [HttpPut]
        [Authorize] // Require authentication
        public async Task<ActionResult<BlogPostModel>> EditBlog([FromBody] BlogPostModel blog)
        {
            var blogToEdit = await _context.BlogPostTable.FirstOrDefaultAsync(t => t.Id == blog.Id);

            if(blogToEdit == null)
            {
                return NotFound("The item you're trying to edit does not exist");
            }

            // Check if the authenticated user owns this blog
            var userId = GetUserId();
            if (userId == 0 || blogToEdit.UserId != userId)
                return Forbid("You can only edit your own blogs");

            blogToEdit.Content = blog.Content;
            blogToEdit.Title = blog.Title;
            
            await _context.SaveChangesAsync();
            return Ok(blogToEdit);
        }

        [HttpDelete("{id}")]
        [Authorize] // Require authentication
        public async Task<ActionResult> DeleteBlog(int id)
        {
            var blogToDelete = _context.BlogPostTable.FirstOrDefault(t => t.Id == id);

            if (blogToDelete == null)
            {
                return NotFound("Item does not exist");
            }
            
            // Check if the authenticated user owns this blog
            var userId = GetUserId();
            if (userId == 0 || blogToDelete.UserId != userId)
                return Forbid("You can only delete your own blogs");

            _context.BlogPostTable.Remove(blogToDelete);
            await _context.SaveChangesAsync();
            return Ok("Item successfully deleted");
        }
        
        [HttpGet]
        [Authorize] // Require authentication
        public async Task<IActionResult> GetMyBlogs()
        {
            var userId = GetUserId();
            if (userId == 0)
                return Unauthorized("Invalid user token");
                
            var blogs = await _context.BlogPostTable
                .Where(b => b.UserId == userId)
                .ToListAsync();
                
            return Ok(blogs);
        }

        // Helper method to get the authenticated user's ID from claims
        private int GetUserId()
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            if (identity == null)
                return 0;

            var userIdClaim = identity.FindFirst("id");
            if (userIdClaim == null)
                return 0;

            if (int.TryParse(userIdClaim.Value, out int userId))
                return userId;
                
            return 0;
        }
    }
}
