// Models/Comment.cs
using System.ComponentModel.DataAnnotations;

namespace BlogProject.Models
{
    public class Comment
    {
        public int Id { get; set; }
        
        [Required]
        public int BlogEntryId { get; set; }
        
        public int? ParentCommentId { get; set; } // Null = ana yorum, değer = cevap yorumu
        
        public int? UserId { get; set; } // Null = misafir kullanıcı
        
        // Misafir kullanıcı bilgileri
        public string? GuestName { get; set; }
        public string? GuestEmail { get; set; }
        
        [Required]
        [StringLength(5000)]
        public string Content { get; set; } = string.Empty; // Markdown formatında
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? UpdatedAt { get; set; }
        
        // Navigation Properties
        public BlogEntry BlogEntry { get; set; } = null!;
        public Comment? ParentComment { get; set; }
        public List<Comment> Replies { get; set; } = new();
        public User? User { get; set; } // Null olabilir (misafir kullanıcılar için)
    }
}