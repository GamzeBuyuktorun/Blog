using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlogProject.Models
{
    public class BlogEntry
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int BlogId { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        [StringLength(100)]
        [Column(TypeName = "varchar(100)")]
        public string Slug { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        [Column(TypeName = "varchar(200)")]
        public string Title { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "TEXT")]
        public string Content { get; set; } = string.Empty; // Markdown içerik

        [Column(TypeName = "TEXT")]
        public string HtmlContent { get; set; } = string.Empty; // HTML'e dönüştürülmüş içerik

        // Foreign Key
        [ForeignKey("BlogId")]
        public virtual Blog Blog { get; set; } = null!;
    }
}