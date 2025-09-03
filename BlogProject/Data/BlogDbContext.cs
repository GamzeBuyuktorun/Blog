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

        public DbSet<Comment> Comments => Set<Comment>();


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


            // Comment yapılandırması
            modelBuilder.Entity<Comment>(entity =>
            {
                // BlogEntry ile ilişki
                entity.HasOne(c => c.BlogEntry)
                      .WithMany(b => b.Comments)
                      .HasForeignKey(c => c.BlogEntryId)
                      .OnDelete(DeleteBehavior.Cascade);
                
                // User ile ilişki (opsiyonel - misafir kullanıcılar için)
                entity.HasOne(c => c.User)
                      .WithMany()
                      .HasForeignKey(c => c.UserId)
                      .OnDelete(DeleteBehavior.SetNull); // User silinirse UserId null olur
                
                // Self-referencing relationship (Parent-Child yorumlar)
                entity.HasOne(c => c.ParentComment)
                      .WithMany(c => c.Replies)
                      .HasForeignKey(c => c.ParentCommentId)
                      .OnDelete(DeleteBehavior.Cascade); // Parent silinirse children da silinir
                
                // İndeksler
                entity.HasIndex(c => c.BlogEntryId)
                      .HasDatabaseName("idx_comment_blog_entry_id");
                entity.HasIndex(c => c.ParentCommentId)
                      .HasDatabaseName("idx_comment_parent_id");
                entity.HasIndex(c => c.CreatedAt)
                      .HasDatabaseName("idx_comment_created_at");
            });
        }
    }
}

