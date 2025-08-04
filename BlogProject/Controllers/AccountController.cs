using BlogProject.Data;
using BlogProject.Models;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;

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

            if (user.PasswordHash != passwordConfirm)
            {
                ModelState.AddModelError("", "Şifreler uyuşmuyor.");
                return View(user);
            }

            if (await _context.Users.AnyAsync(u => u.Email == user.Email || u.UserName == user.UserName))
            {
                ModelState.AddModelError("", "Bu email veya kullanıcı adı zaten kullanılıyor.");
                return View(user);
            }

            user.PasswordHash = HashPassword(user.PasswordHash);
            user.CreatedAt = DateTime.UtcNow;

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return RedirectToAction("Login");
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
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null || !VerifyPassword(password, user.PasswordHash))
            {
                ModelState.AddModelError("", "Email veya şifre hatalı.");
                return View();
            }

            // oturum açılıyor
            HttpContext.Session.SetInt32("UserId", user.Id);
            HttpContext.Session.SetString("UserName", user.UserName);

            return RedirectToAction("Index", "Home");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        private string HashPassword(string password)
        {
            using var hmac = new HMACSHA512();
            var salt = hmac.Key;
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(salt) + ":" + Convert.ToBase64String(hash);
        }

        private bool VerifyPassword(string password, string hashedPassword)
        {
            var parts = hashedPassword.Split(':');
            if (parts.Length != 2) return false;

            var salt = Convert.FromBase64String(parts[0]);
            var hash = Convert.FromBase64String(parts[1]);

            using var hmac = new HMACSHA512(salt);
            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            return computedHash.SequenceEqual(hash);
        }
    }
}
