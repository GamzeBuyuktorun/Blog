using BlogProject.Models;
using Microsoft.EntityFrameworkCore;

namespace BlogProject.Data
{
    public class BlogDbContext : DbContext
    {
        public BlogDbContext(DbContextOptions<BlogDbContext> options) : base(options) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<Blog> Blogs => Set<Blog>();
        public DbSet<BlogEntry> BlogEntries => Set<BlogEntry>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User: Username ve Email unique
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique()
                .HasDatabaseName("uniq_user_username");

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique()
                .HasDatabaseName("uniq_user_email");

            // Blog: (OwnerId, Slug) unique
            modelBuilder.Entity<Blog>()
                .HasIndex(b => new { b.OwnerId, b.Slug })
                .IsUnique()
                .HasDatabaseName("uniq_blog_owner_id_slug");

            // BlogEntry: (BlogId, Slug) unique
            modelBuilder.Entity<BlogEntry>()
                .HasIndex(e => new { e.BlogId, e.Slug })
                .IsUnique()
                .HasDatabaseName("uniq_blog_message_blog_id_slug");

            // User ↔ Blog
            modelBuilder.Entity<User>()
                .HasMany(u => u.Blogs)
                .WithOne(b => b.Owner)
                .HasForeignKey(b => b.OwnerId)
                .OnDelete(DeleteBehavior.Cascade);

            // Blog ↔ BlogEntry
            modelBuilder.Entity<Blog>()
                .HasMany(b => b.BlogEntries)
                .WithOne(e => e.Blog)
                .HasForeignKey(e => e.BlogId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
