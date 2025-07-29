using System.ComponentModel.DataAnnotations;

namespace BlogProject.DTOs
{
    public class LoginDto
    {
        [Required(ErrorMessage = "Kullanıcı adı veya e-posta zorunludur")]
        public string UsernameOrEmail { get; set; } = string.Empty;

        [Required(ErrorMessage = "Şifre zorunludur")]
        public string Password { get; set; } = string.Empty;
    }
}