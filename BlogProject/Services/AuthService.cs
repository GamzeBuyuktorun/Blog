using BlogProject.DTOs;
using BlogProject.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BlogProject.Services
{
    public interface IAuthService
    {
        Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto);
        Task<AuthResponseDto> LoginAsync(LoginDto loginDto);
    }

    public class AuthService : IAuthService
    {
        private readonly BlogDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthService(BlogDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto)
        {
            try
            {
                // Kullanıcı adı kontrolü
                var existingUserByUsername = await _context.Users
                    .FirstOrDefaultAsync(u => u.Username == registerDto.Username);

                if (existingUserByUsername != null)
                {
                    return new AuthResponseDto
                    {
                        Success = false,
                        Message = "Bu kullanıcı adı zaten kullanılmaktadır"
                    };
                }

                // E-posta kontrolü
                var existingUserByEmail = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == registerDto.Email);

                if (existingUserByEmail != null)
                {
                    return new AuthResponseDto
                    {
                        Success = false,
                        Message = "Bu e-posta adresi zaten kullanılmaktadır"
                    };
                }

                // Yeni kullanıcı oluştur
                var user = new User
                {
                    Username = registerDto.Username,
                    Email = registerDto.Email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password),
                    CreatedAt = DateTime.UtcNow
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                // JWT token oluştur
                var token = GenerateJwtToken(user);

                return new AuthResponseDto
                {
                    Success = true,
                    Message = "Kayıt başarılı",
                    Token = token,
                    User = new UserDto
                    {
                        Id = user.Id,
                        Username = user.Username,
                        Email = user.Email,
                        CreatedAt = user.CreatedAt
                    }
                };
            }
            catch (Exception ex)
            {
                return new AuthResponseDto
                {
                    Success = false,
                    Message = "Kayıt sırasında bir hata oluştu: " + ex.Message
                };
            }
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto loginDto)
        {
            try
            {
                // Kullanıcıyı username veya email ile bul
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Username == loginDto.UsernameOrEmail || 
                                             u.Email == loginDto.UsernameOrEmail);

                if (user == null)
                {
                    return new AuthResponseDto
                    {
                        Success = false,
                        Message = "Kullanıcı bulunamadı"
                    };
                }

                // Şifre kontrolü
                if (!BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
                {
                    return new AuthResponseDto
                    {
                        Success = false,
                        Message = "Hatalı şifre"
                    };
                }

                // JWT token oluştur
                var token = GenerateJwtToken(user);

                return new AuthResponseDto
                {
                    Success = true,
                    Message = "Giriş başarılı",
                    Token = token,
                    User = new UserDto
                    {
                        Id = user.Id,
                        Username = user.Username,
                        Email = user.Email,
                        CreatedAt = user.CreatedAt
                    }
                };
            }
            catch (Exception ex)
            {
                return new AuthResponseDto
                {
                    Success = false,
                    Message = "Giriş sırasında bir hata oluştu: " + ex.Message
                };
            }
        }

        private string GenerateJwtToken(User user)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"];
            var issuer = jwtSettings["Issuer"];
            var audience = jwtSettings["Audience"];
            var expiryMinutes = int.Parse(jwtSettings["ExpiryMinutes"]);

            var key = Encoding.ASCII.GetBytes(secretKey);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Email, user.Email)
                }),
                Expires = DateTime.UtcNow.AddMinutes(expiryMinutes),
                Issuer = issuer,
                Audience = audience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), 
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}