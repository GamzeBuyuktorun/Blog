using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

// Models/Blog.cs
namespace BlogProject.Models
{
    public class Blog
    {
        public int Id { get; set; }

        public int OwnerId { get; set; }
        public User Owner { get; set; } = null!;

        [Required(ErrorMessage = "Blog başlığı gereklidir")]
        [MaxLength(150)]
        public string Title { get; set; } = "";

        [MaxLength(500)]
        public string? Description { get; set; }

        [MaxLength(160)]
        public string Slug { get; set; } = "";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Görüntüleme sayısı
        public int ViewCount { get; set; } = 0;

        public ICollection<BlogEntry> BlogEntries { get; set; } = new List<BlogEntry>();
    }
}