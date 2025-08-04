using BlogProject.Data;
using BlogProject.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BlogProject.Controllers
{
    [Authorize]
    public class BlogController : Controller
    {
        private readonly BlogDbContext _context;

        public BlogController(BlogDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> MyBlogs()
        {
            var userName = User.Identity.Name;
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == userName);
            if (user == null) return NotFound();

            var blogs = await _context.Blogs
                .Where(b => b.OwnerId == user.Id)
                .OrderByDescending(b => b.UpdatedAt)
                .ToListAsync();

            return View(blogs);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Blog blog)
        {
            var userName = User.Identity.Name;
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == userName);
            if (user == null) return NotFound();

            var slug = GenerateSlug(blog.Title);
            var exists = await _context.Blogs.AnyAsync(b => b.OwnerId == user.Id && b.Slug == slug);
            if (exists)
            {
                ModelState.AddModelError("Slug", "Aynı slug’a sahip bir blog zaten var.");
                return View(blog);
            }

            blog.Slug = slug;
            blog.OwnerId = user.Id;
            blog.CreatedAt = DateTime.UtcNow;
            blog.UpdatedAt = DateTime.UtcNow;

            _context.Blogs.Add(blog);
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

        [HttpGet]
public async Task<IActionResult> Edit(int id)
{
    var userName = User.Identity.Name;
    var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == userName);
    if (user == null) return NotFound();

    var blog = await _context.Blogs.FirstOrDefaultAsync(b => b.Id == id && b.OwnerId == user.Id);
    if (blog == null) return NotFound();

    return View(blog);
}

[HttpPost]
public async Task<IActionResult> Edit(int id, Blog updatedBlog)
{
    var userName = User.Identity.Name;
    var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == userName);
    if (user == null) return NotFound();

    var blog = await _context.Blogs.FirstOrDefaultAsync(b => b.Id == id && b.OwnerId == user.Id);
    if (blog == null) return NotFound();

    // Slug güncellenecekse, aynı slug zaten var mı kontrol et
    var newSlug = GenerateSlug(updatedBlog.Title);
    var duplicate = await _context.Blogs
        .AnyAsync(b => b.OwnerId == user.Id && b.Slug == newSlug && b.Id != id);

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
        .Include(b => b.Posts.OrderByDescending(p => p.CreatedAt))
        .Include(b => b.Owner)
        .FirstOrDefaultAsync(b => b.Id == id);

    if (blog == null) return NotFound();

    return View(blog);
}
[HttpGet]
public async Task<IActionResult> Delete(int id)
{
    var userName = User.Identity.Name;
    var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == userName);
    if (user == null) return NotFound();

    var blog = await _context.Blogs.FirstOrDefaultAsync(b => b.Id == id && b.OwnerId == user.Id);
    if (blog == null) return NotFound();

    return View(blog);
}

[HttpPost, ActionName("Delete")]
public async Task<IActionResult> DeleteConfirmed(int id)
{
    var userName = User.Identity.Name;
    var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == userName);
    if (user == null) return NotFound();

    var blog = await _context.Blogs
        .Include(b => b.Posts)
        .FirstOrDefaultAsync(b => b.Id == id && b.OwnerId == user.Id);

    if (blog == null) return NotFound();

    _context.Posts.RemoveRange(blog.Posts); // Blog yazılarını sil
    _context.Blogs.Remove(blog);            // Blog'u sil
    await _context.SaveChangesAsync();

    return RedirectToAction("MyBlogs");
}


    }
}
