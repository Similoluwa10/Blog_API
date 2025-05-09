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
using Microsoft.EntityFrameworkCore.Query;


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
        [Authorize]
        public async Task<ActionResult> CreateBlog([FromBody] BlogPostModel newBlog)
        {
            var userId = GetUserId();
            if(userId == 0)
            {
                return BadRequest("nvalid user token");
            }

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

        [HttpGet]
        [Authorize]
        public async Task<IEnumerable<BlogPostModel>> GetMyBlogs()
        {
            var userId = GetUserId();
            return await _context.BlogPostTable.Where(t => t.UserId == userId).ToListAsync();            
        }

        
        [HttpPut]
        [Authorize]
        public async Task<ActionResult<BlogPostModel>> EditBlog([FromBody] BlogPostModel blog)
        {
            var blogToEdit = await _context.BlogPostTable.FirstOrDefaultAsync(t => t.Id == blog.Id);

            if(blogToEdit == null)
            {
                return NotFound("The item you're trying to edit does not exist");
            }

            var userId = GetUserId();
            if (userId == 0 || userId != blogToEdit.UserId)
                return BadRequest("You can only edit your own blogs!!");

            blogToEdit.Title = blog.Title;
            blogToEdit.Content = blog.Content;            
            
            await _context.SaveChangesAsync();
            return Ok(blogToEdit);
        }



        
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<ActionResult> DeleteBlog(int id)
        {
            var blogToDelete = _context.BlogPostTable.FirstOrDefault(t => t.Id == id);

            //check if null input
            if (blogToDelete == null)
            {
                return NotFound("Item does not exist");
            }

            //check if input type is correct
            if (typeof(int) != id.GetType())
            {
                return BadRequest("Wrong input type");
            }

            //check if user owns blog
            var userId = GetUserId();
            if (userId == 0 || userId != blogToDelete.UserId)
                return BadRequest("You can only delete your own blogs");            

                      
            _context.BlogPostTable.Remove(blogToDelete);
            await _context.SaveChangesAsync();
            return Ok("Item successfully deleted");
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
