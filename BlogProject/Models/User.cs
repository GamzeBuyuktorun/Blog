using System.ComponentModel.DataAnnotations;

namespace BlogProject.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required, MaxLength(50)]
        public string Username { get; set; } = "";

        [Required, MaxLength(120), EmailAddress]
        public string Email { get; set; } = "";

        [Required]
        public string PasswordHash { get; set; } = "";   // düz hash metni

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public ICollection<Blog> Blogs { get; set; } = new List<Blog>();
    }
}
