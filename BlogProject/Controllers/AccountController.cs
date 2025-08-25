using BlogProject.Data;
using BlogProject.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace BlogProject.Controllers
{
    public class AccountController : Controller
    {
        private readonly BlogDbContext _context;

        public AccountController(BlogDbContext context)
        {
            _context = context;
        }

        // GET: /Account/Register
        public IActionResult Register()
        {
            return View();
        }

        // POST: /Account/Register
        [HttpPost]
        public async Task<IActionResult> Register(User user, string passwordConfirm)
        {
            if (!ModelState.IsValid)
                return View(user);

            // user.PasswordHash burada "düz şifre" olarak geliyor (ViewModel yoksa)
            if (string.IsNullOrWhiteSpace(user.PasswordHash) || user.PasswordHash != passwordConfirm)
            {
                ModelState.AddModelError("", "Şifreler uyuşmuyor.");
                return View(user);
            }

            // Username & Email benzersiz olmalı
            if (await _context.Users.AnyAsync(u => u.Email == user.Email || u.Username == user.Username))
            {
                ModelState.AddModelError("", "Bu e-posta veya kullanıcı adı zaten kullanılıyor.");
                return View(user);
            }

            // Kullanıcı adı boşsa e-postadan türet
            if (string.IsNullOrWhiteSpace(user.Username))
            {
                var baseUsername = user.Email.Split('@')[0];
                var username = baseUsername;
                int count = 1;

                while (await _context.Users.AnyAsync(u => u.Username == username))
                {
                    username = $"{baseUsername}{count++}";
                }

                user.Username = username;
            }

            // Şifreyi hash'le (salt:hash formatı)
            user.PasswordHash = HashPassword(user.PasswordHash);
            user.CreatedAt = DateTime.UtcNow;

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Opsiyonel: kayıt sonrası login
            HttpContext.Session.SetInt32("UserId", user.Id);
            HttpContext.Session.SetString("Username", user.Username);

            return RedirectToAction("Index", "Home");
        }

        // GET: /Account/Login
        public IActionResult Login()
        {
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                ModelState.AddModelError("", "E-posta ve şifre zorunludur.");
                return View();
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null || !VerifyPassword(password, user.PasswordHash))
            {
                ModelState.AddModelError("", "E-posta veya şifre hatalı.");
                return View();
            }

            HttpContext.Session.SetInt32("UserId", user.Id);
            HttpContext.Session.SetString("Username", user.Username);

            return RedirectToAction("Index", "Home");
        }

        // POST: /Account/Logout
        [HttpPost]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        // Şifre hash'leme (salt:hash)
        private string HashPassword(string password)
        {
            using var hmac = new HMACSHA512();
            var salt = hmac.Key;
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            return $"{Convert.ToBase64String(salt)}:{Convert.ToBase64String(hash)}";
        }

        // Şifre doğrulama
        private bool VerifyPassword(string password, string hashedPassword)
        {
            var parts = hashedPassword?.Split(':');
            if (parts == null || parts.Length != 2) return false;

            var salt = Convert.FromBase64String(parts[0]);
            var hash = Convert.FromBase64String(parts[1]);

            using var hmac = new HMACSHA512(salt);
            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));

            return computedHash.SequenceEqual(hash);
        }
    }
}
