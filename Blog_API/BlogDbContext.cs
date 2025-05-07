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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure the relationship between BlogPost and User
            modelBuilder.Entity<BlogPostModel>()
                .HasOne(b => b.User)
                .WithMany(u => u.BlogPosts)
                .HasForeignKey(b => b.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}