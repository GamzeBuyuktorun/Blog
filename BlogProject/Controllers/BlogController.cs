using BlogProject.Data;
using BlogProject.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BlogProject.Controllers
{
    public class BlogController : Controller
    {
        private readonly BlogDbContext _context;

        public BlogController(BlogDbContext context)
        {
            _context = context;
        }

        private int? GetCurrentUserId()
        {
            return HttpContext.Session.GetInt32("UserId");
        }

        public async Task<IActionResult> MyBlogs()
        {
            var userId = GetCurrentUserId();
            if (userId == null) return RedirectToAction("Login", "Account");

            var blogs = await _context.Blogs
                .Where(b => b.OwnerId == userId)
                .OrderByDescending(b => b.UpdatedAt)
                .ToListAsync();

            return View(blogs);
        }

        [HttpGet]
        public IActionResult Create()
        {
            if (GetCurrentUserId() == null) return RedirectToAction("Login", "Account");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Blog blog)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return RedirectToAction("Login", "Account");

            var slug = GenerateSlug(blog.Title);
            var exists = await _context.Blogs.AnyAsync(b => b.OwnerId == userId && b.Slug == slug);
            if (exists)
            {
                ModelState.AddModelError("Slug", "Aynı slug’a sahip bir blog zaten var.");
                return View(blog);
            }

            blog.Slug = slug;
            blog.OwnerId = userId.Value;
            blog.CreatedAt = DateTime.UtcNow;
            blog.UpdatedAt = DateTime.UtcNow;

            _context.Blogs.Add(blog);
            await _context.SaveChangesAsync();

            return RedirectToAction("MyBlogs");
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return RedirectToAction("Login", "Account");

            var blog = await _context.Blogs.FirstOrDefaultAsync(b => b.Id == id && b.OwnerId == userId);
            if (blog == null) return NotFound();

            return View(blog);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, Blog updatedBlog)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return RedirectToAction("Login", "Account");

            var blog = await _context.Blogs.FirstOrDefaultAsync(b => b.Id == id && b.OwnerId == userId);
            if (blog == null) return NotFound();

            var newSlug = GenerateSlug(updatedBlog.Title);
            var duplicate = await _context.Blogs
                .AnyAsync(b => b.OwnerId == userId && b.Slug == newSlug && b.Id != id);

            if (duplicate)
            {
                ModelState.AddModelError("Slug", "Bu başlığa sahip bir blog'unuz zaten var.");
                return View(updatedBlog);
            }

            blog.Title = updatedBlog.Title;
            blog.Slug = newSlug;
            blog.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return RedirectToAction("MyBlogs");
        }

     [HttpGet]
public async Task<IActionResult> Details(int id)
{
    var blog = await _context.Blogs
        .Include(b => b.BlogEntries)  // sade navigation property
      
        .Include(b => b.Owner)
        .FirstOrDefaultAsync(b => b.Id == id);

    if (blog == null) return NotFound();

    // İsteğe bağlı: Post'ları sıralamak için View içinde sıralama yaparız
    blog.BlogEntries = blog.BlogEntries.OrderByDescending(p => p.CreatedAt).ToList();

    return View(blog);
}


        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return RedirectToAction("Login", "Account");

            var blog = await _context.Blogs.FirstOrDefaultAsync(b => b.Id == id && b.OwnerId == userId);
            if (blog == null) return NotFound();

            return View(blog);
        }

        [HttpPost, ActionName("Delete")]
public async Task<IActionResult> DeleteConfirmed(int id)
{
    var userId = GetCurrentUserId();
    if (userId == null) return RedirectToAction("Login", "Account");

    var blog = await _context.Blogs
        .Include(b => b.BlogEntries) // Doğru navigation property
        .FirstOrDefaultAsync(b => b.Id == id && b.OwnerId == userId);

    if (blog == null) return NotFound();

    _context.BlogEntries.RemoveRange(blog.BlogEntries);
    _context.Blogs.Remove(blog);
    await _context.SaveChangesAsync();

    return RedirectToAction("MyBlogs");
}


        private string GenerateSlug(string title)
        {
            var invalids = new[] { "ğ", "ü", "ş", "ı", "ö", "ç", "Ğ", "Ü", "Ş", "İ", "Ö", "Ç" };
            var replaces = new[] { "g", "u", "s", "i", "o", "c", "G", "U", "S", "I", "O", "C" };
            for (int i = 0; i < invalids.Length; i++)
            {
                title = title.Replace(invalids[i], replaces[i]);
            }

            return System.Text.RegularExpressions.Regex.Replace(title.ToLower(), @"[^a-z0-9\s-]", "").Replace(" ", "-");
        }
    }
}
