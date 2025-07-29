using BlogProject.DTOs;
using BlogProject.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BlogProject.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        /// <summary>
        /// Kullanıcı kaydı
        /// </summary>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new AuthResponseDto
                {
                    Success = false,
                    Message = "Geçersiz veri",
                });
            }

            var result = await _authService.RegisterAsync(registerDto);

            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        /// <summary>
        /// Kullanıcı girişi
        /// </summary>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new AuthResponseDto
                {
                    Success = false,
                    Message = "Geçersiz veri"
                });
            }

            var result = await _authService.LoginAsync(loginDto);

            if (result.Success)
            {
                return Ok(result);
            }

            return Unauthorized(result);
        }

        /// <summary>
        /// Kullanıcı çıkışı (Token invalidation - client tarafında token silinmeli)
        /// </summary>
        [HttpPost("logout")]
        [Authorize]
        public IActionResult Logout()
        {
            // JWT stateless olduğu için logout işlemi genelde client tarafında yapılır
            // Token'ı client tarafında silmek yeterlidir
            // İsterseniz burada blacklist mantığı da kurabilirsiniz
            
            return Ok(new AuthResponseDto
            {
                Success = true,
                Message = "Çıkış başarılı. Token'ı client tarafında siliniz."
            });
        }

        /// <summary>
        /// Mevcut kullanıcı bilgilerini getir
        /// </summary>
        [HttpGet("me")]
        [Authorize]
        public IActionResult GetCurrentUser()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var username = User.FindFirst(ClaimTypes.Name)?.Value;
            var email = User.FindFirst(ClaimTypes.Email)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new AuthResponseDto
                {
                    Success = false,
                    Message = "Geçersiz token"
                });
            }

            return Ok(new AuthResponseDto
            {
                Success = true,
                Message = "Kullanıcı bilgileri",
                User = new UserDto
                {
                    Id = int.Parse(userId),
                    Username = username ?? "",
                    Email = email ?? "",
                    CreatedAt = DateTime.UtcNow // Bu bilgi token'da yok, gerçek uygulamada DB'den çekilmeli
                }
            });
        }

        /// <summary>
        /// Token doğrulama endpoint'i
        /// </summary>
        [HttpGet("validate")]
        [Authorize]
        public IActionResult ValidateToken()
        {
            return Ok(new AuthResponseDto
            {
                Success = true,
                Message = "Token geçerli"
            });
        }
    }
}