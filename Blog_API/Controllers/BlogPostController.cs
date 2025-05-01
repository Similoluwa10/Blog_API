using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Blog_API.Context;
using Blog_API.Models;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Threading.Tasks;


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
        public async Task<ActionResult> CreateBlog([FromBody] BlogPostModel newBlog)
        {
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
        public async Task<ActionResult<BlogPostModel>> EditBlog([FromBody] BlogPostModel blog)
        {
            var blogToEdit = await _context.BlogPostTable.FirstOrDefaultAsync(t => t.Id == blog.Id);

            if(blogToEdit == null)
            {
                return NotFound("The item you're trying to edit does not exist");
            }

            blogToEdit.Content = blog.Content;
            
            await _context.SaveChangesAsync();
            return Ok(blogToEdit);
        }


        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteBlog(int id)
        {
            var blogToDelete = _context.BlogPostTable.FirstOrDefault(t => t.Id == id);

            if (blogToDelete == null)
            {
                return NotFound("Item does not exist");
            }
            if(typeof(int) != id.GetType())
            {
                return BadRequest("Wrong input type");
            }
           
            _context.BlogPostTable.Remove(blogToDelete);
            await _context.SaveChangesAsync();
            return Ok("Item successfully deleted");
        }
    }

}
