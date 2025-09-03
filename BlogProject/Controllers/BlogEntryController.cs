using BlogProject.Data;
using BlogProject.Models;
using Markdig;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BlogProject.Controllers
{
    public class BlogEntryController : Controller
    {
        private readonly BlogDbContext _context;

        public BlogEntryController(BlogDbContext context)
        {
            _context = context;
        }

        private int? GetCurrentUserId()
        {
            return HttpContext.Session.GetInt32("UserId");
        }

        // GET: /BlogEntry/Create?blogId=123
        [HttpGet]
        public async Task<IActionResult> Create(int blogId)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return RedirectToAction("Login", "Account");

            var blog = await _context.Blogs
                .FirstOrDefaultAsync(b => b.Id == blogId && b.OwnerId == userId);

            if (blog == null) return Unauthorized();

            ViewBag.BlogId = blogId;
            ViewBag.BlogTitle = blog.Title;
            return View();
        }

        // POST: /BlogEntry/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int blogId, BlogEntry entry)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return RedirectToAction("Login", "Account");

            var blog = await _context.Blogs
                .FirstOrDefaultAsync(b => b.Id == blogId && b.OwnerId == userId);

            if (blog == null) return Unauthorized();

            // ÇÖZÜM: Slug hatalarını ModelState'den kaldır
            ModelState.Remove("Slug");
            ModelState.Remove("Blog");

            if (!ModelState.IsValid)
            {
                ViewBag.BlogId = blogId;
                ViewBag.BlogTitle = blog.Title;
                return View(entry);
            }

            // null güvenliği
            var safeTitle = entry.Title ?? string.Empty;
            var safeContent = entry.Content ?? string.Empty;

            var slug = GenerateSlug(safeTitle);

            var duplicate = await _context.BlogEntries
                .AnyAsync(e => e.BlogId == blogId && e.Slug == slug);

            if (duplicate)
            {

                ModelState.AddModelError(string.Empty, "Aynı başlığa sahip bir yazı zaten var.");

                ViewBag.BlogId = blogId;
                ViewBag.BlogTitle = blog.Title;
                return View(entry);
            }

            entry.BlogId = blogId;
            entry.Slug = slug;
            entry.CreatedAt = DateTime.UtcNow;
            entry.UpdatedAt = DateTime.UtcNow;
            entry.HtmlContent = Markdown.ToHtml(safeContent);

            _context.BlogEntries.Add(entry);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Blog yazısı başarıyla oluşturuldu!";
            return RedirectToAction("Details", "Blog", new { slug = blog.Slug });
        }


        // GET: /Entry/{slug} - URL'den slug ile erişim
        [HttpGet("/Entry/{slug}")]
        public async Task<IActionResult> Details(string slug)
        {
            var entry = await _context.BlogEntries
                .Include(e => e.Blog)
                .ThenInclude(b => b.Owner)
                .Include(e => e.Comments)
                .ThenInclude(c => c.User)
                .Include(e => e.Comments)
                .ThenInclude(c => c.ParentComment)
                .Include(e => e.Comments)
                .ThenInclude(c => c.Replies)
                .ThenInclude(r => r.User)
                .FirstOrDefaultAsync(e => e.Slug == slug);

            if (entry == null) return NotFound();

            return View(entry);
        }

        // GET: /BlogEntry/Details/{id} - ID ile erişim (yorum sistemi için)
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var entry = await _context.BlogEntries
                .Include(e => e.Blog)
                .ThenInclude(b => b.Owner)
                .Include(e => e.Comments)
                .ThenInclude(c => c.User)
                .Include(e => e.Comments)
                .ThenInclude(c => c.ParentComment)
                .Include(e => e.Comments)
                .ThenInclude(c => c.Replies)
                .ThenInclude(r => r.User)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (entry == null) return NotFound();


            return View(entry);
        }

        // GET: /BlogEntry/Edit/{id}
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {

            var userId = GetCurrentUserId();
            if (userId == null) return RedirectToAction("Login", "Account");

            var entry = await _context.BlogEntries
                .Include(e => e.Blog)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (entry == null) return NotFound();

            // Sadece blog sahibi düzenleyebilir
            if (entry.Blog.OwnerId != userId.Value)
            {
                return Unauthorized();
            }

            ViewBag.BlogId = entry.BlogId;
            ViewBag.BlogTitle = entry.Blog.Title;
            return View(entry);
        }

        // POST: /BlogEntry/Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, BlogEntry entry)
        {

            var userId = GetCurrentUserId();
            if (userId == null) return RedirectToAction("Login", "Account");

            if (id != entry.Id)
            {
                return NotFound();
            }

            var existingEntry = await _context.BlogEntries
                .Include(e => e.Blog)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (existingEntry == null) return NotFound();

            // Sadece blog sahibi düzenleyebilir
            if (existingEntry.Blog.OwnerId != userId.Value)
            {
                return Unauthorized();
            }

            // ModelState'den otomatik doldurulan alanların hatalarını temizle
            ModelState.Remove("Blog");
            ModelState.Remove("BlogId");
            ModelState.Remove("CreatedAt");
            ModelState.Remove("UpdatedAt");
            ModelState.Remove("Slug");
            ModelState.Remove("HtmlContent");

            if (!ModelState.IsValid)
            {
                ViewBag.BlogId = existingEntry.BlogId;
                ViewBag.BlogTitle = existingEntry.Blog.Title;
                return View(entry);
            }

            try
            {
                // null güvenliği
                var safeTitle = entry.Title ?? string.Empty;
                var safeContent = entry.Content ?? string.Empty;

                existingEntry.Title = entry.Title;
                existingEntry.Content = entry.Content;
                existingEntry.UpdatedAt = DateTime.UtcNow;
                existingEntry.HtmlContent = Markdown.ToHtml(safeContent);

                // Başlık değişirse slug'ı da güncelle
                var newSlug = GenerateSlug(safeTitle);
                if (existingEntry.Slug != newSlug)
                {
                    // Yeni slug'ın blog içinde benzersiz olduğundan emin ol
                    var existingSlug = await _context.BlogEntries
                        .AnyAsync(e => e.BlogId == existingEntry.BlogId && e.Slug == newSlug && e.Id != id);



                    if (!existingSlug)
                    {
                        existingEntry.Slug = newSlug;
                    }
                    else
                    {
                        existingEntry.Slug = newSlug + "-" + DateTime.UtcNow.Ticks.ToString()[^6..];
                    }
                }

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Blog yazısı başarıyla güncellendi!";
                return RedirectToAction("Details", new { slug = existingEntry.Slug });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Blog yazısı güncellenirken bir hata oluştu.");
                ViewBag.BlogId = existingEntry.BlogId;
                ViewBag.BlogTitle = existingEntry.Blog.Title;
                return View(entry);
            }
        }

        // POST: /BlogEntry/Delete/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {

            var userId = GetCurrentUserId();
            if (userId == null) return RedirectToAction("Login", "Account");

            var entry = await _context.BlogEntries
                .Include(e => e.Blog)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (entry == null) return NotFound();

            // Sadece blog sahibi silebilir
            if (entry.Blog.OwnerId != userId.Value)
            {
                return Unauthorized();
            }

            var blogSlug = entry.Blog.Slug;

            _context.BlogEntries.Remove(entry);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Blog yazısı başarıyla silindi!";
            return RedirectToAction("Details", "Blog", new { slug = blogSlug });
        }


        // Yorumları açma/kapatma
        [HttpPost]
        public async Task<IActionResult> ToggleComments(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var blogEntry = await _context.BlogEntries
                .Include(b => b.Blog)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (blogEntry == null)
            {
                return NotFound();
            }

            // Sadece blog sahibi yorumları açıp/kapatabilir
            if (blogEntry.Blog?.OwnerId != userId)
            {
                TempData["ErrorMessage"] = "Bu işlemi yapma yetkiniz yok.";
                return RedirectToAction("Details", new { id });
            }

            blogEntry.CommentsEnabled = !blogEntry.CommentsEnabled;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = blogEntry.CommentsEnabled ? 
                "Yorumlar açıldı." : "Yorumlar kapatıldı. Mevcut yorumlar gizlendi.";

            return RedirectToAction("Details", new { id });
        }


        private string GenerateSlug(string title)
        {
            var invalids = new[] { "ğ", "ü", "ş", "ı", "ö", "ç", "Ğ", "Ü", "Ş", "İ", "Ö", "Ç" };
            var replaces = new[] { "g", "u", "s", "i", "o", "c", "G", "U", "S", "I", "O", "C" };
            for (int i = 0; i < invalids.Length; i++)
            {
                title = title.Replace(invalids[i], replaces[i]);
            }

            return System.Text.RegularExpressions.Regex
                .Replace(title.ToLower(), @"[^a-z0-9\s-]", "")
                .Replace(" ", "-");
        }
    }
}