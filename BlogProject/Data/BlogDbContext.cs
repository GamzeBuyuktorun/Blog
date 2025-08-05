using BlogProject.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BlogProject.Data
{
    public class BlogDbContext : IdentityDbContext<User, IdentityRole<int>, int>
    {
        public BlogDbContext(DbContextOptions<BlogDbContext> options) : base(options) { }

        public DbSet<Blog> Blogs { get; set; }
        public DbSet<BlogEntry> Posts { get; set; }
        public DbSet<BlogEntry> BlogEntries { get; set; }

        


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); // Identity şeması

            modelBuilder.Entity<Blog>()
                .HasIndex(b => new { b.OwnerId, b.Slug })
                .IsUnique()
                .HasDatabaseName("uniq_blog_owner_id_slug");

            modelBuilder.Entity<BlogEntry>()
                .HasIndex(e => new { e.BlogId, e.Slug })
                .IsUnique()
                .HasDatabaseName("uniq_blog_message_blog_id_slug");

            modelBuilder.Entity<User>()
                .HasMany<Blog>()
                .WithOne(b => b.Owner)
                .HasForeignKey(b => b.OwnerId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Blog>()
                .HasMany<BlogEntry>()
                .WithOne(e => e.Blog)
                .HasForeignKey(e => e.BlogId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
