using System.ComponentModel.DataAnnotations;

namespace BlogProject.Models
{
    public class BlogEntry
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "Başlık gereklidir")]
        [StringLength(200, ErrorMessage = "Başlık en fazla 200 karakter olabilir")]
        public string Title { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "İçerik gereklidir")]
        public string Content { get; set; } = string.Empty;
        
        // Slug otomatik oluşturuluyor, Required olmamalı
        public string Slug { get; set; } = string.Empty;
        
        public string? HtmlContent { get; set; }
        
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        
        // Foreign Key
        public int BlogId { get; set; }
        
        // Navigation Property - Required olmamalı
        public virtual Blog? Blog { get; set; }
    }
}