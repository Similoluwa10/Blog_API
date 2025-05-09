using Blog_API.Models;
using Microsoft.EntityFrameworkCore;


namespace Blog_API.Context
{
    public class BlogDbContext : DbContext
    {
        public BlogDbContext(DbContextOptions options) : base(options)
        {

        }

        // configure relationship between blogpost and user
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<BlogPostModel>()
                .HasOne(s => s.User)
                .WithMany(t => t.BlogPosts)
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }

        public DbSet<BlogPostModel> BlogPostTable { get; set; }
        public DbSet<UserModel> UserTable { get; set; }
        public DbSet<BlackListTokensModel> BlackListToken { get; set; }
    }
}