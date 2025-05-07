using Blog_API.Models;
using Microsoft.EntityFrameworkCore;


namespace Blog_API.Context
{
    public class BlogDbContext : DbContext
    {
        public BlogDbContext(DbContextOptions options) : base(options)
        {

        }

        public DbSet<BlogPostModel> BlogPostTable { get; set; }
        public DbSet<UserModel> UserTable { get; set; }
    }
}