using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BlogProject.Data;
using BlogProject.Models;
using System.Text;

namespace BlogProject.Controllers
{
    public class BlogController : Controller
    {
        private readonly BlogDbContext _context;

        public BlogController(BlogDbContext context)
        {
            _context = context;
        }

        // GET: Blog/Index - Tüm blogları listele
        public async Task<IActionResult> Index()
        {
            Console.WriteLine("=== Index action çağrıldı ===");
            
            var blogs = await _context.Blogs
                .Include(b => b.Owner)
                .Include(b => b.BlogEntries)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();

            Console.WriteLine($"Index - Toplam blog sayısı: {blogs.Count}");
            
            foreach (var blog in blogs)
            {
                Console.WriteLine($"Blog: {blog.Title} - Owner: {blog.Owner?.Username}");
            }

            return View(blogs);
        }

        // TEST ACTION
        public IActionResult Test()
        {
            Console.WriteLine("=== Test action çağrıldı ===");
            return Content("BlogController çalışıyor! Test başarılı.");
        }

        // GET: Blog/MyBlogs - Kullanıcının kendi bloglarını listele
        public async Task<IActionResult> MyBlogs()
        {
            Console.WriteLine("=== MyBlogs action çağrıldı ===");
            
            var userId = HttpContext.Session.GetInt32("UserId");
            Console.WriteLine($"Session UserId: {userId}");
            
            if (userId == null)
            {
                Console.WriteLine("UserId null - Login'e yönlendiriliyor");
                return RedirectToAction("Login", "Account");
            }

            Console.WriteLine("UserId mevcut - Bloglar çekiliyor");
            
            var myBlogs = await _context.Blogs
                .Include(b => b.BlogEntries)
                .Where(b => b.OwnerId == userId.Value)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();

            Console.WriteLine($"Bulunan blog sayısı: {myBlogs.Count}");
            Console.WriteLine("View döndürülüyor");
            
            return View(myBlogs);
        }

        // GET: Blog/Create - Blog oluşturma formu
        public IActionResult Create()
        {
            Console.WriteLine("=== Create GET action çağrıldı ===");
            
            var userId = HttpContext.Session.GetInt32("UserId");
            Console.WriteLine($"Create - Session UserId: {userId}");
            
            if (userId == null)
            {
                Console.WriteLine("Create - UserId null, Login'e yönlendiriliyor");
                return RedirectToAction("Login", "Account");
            }

            Console.WriteLine("Create - View döndürülüyor");
            return View();
        }

        // POST: Blog/Create - Blog oluşturma
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Blog blog)
        {
            Console.WriteLine("=== Create POST action çağrıldı ===");
            
            var userId = HttpContext.Session.GetInt32("UserId");
            Console.WriteLine($"UserId: {userId}");
            
            if (userId == null)
            {
                Console.WriteLine("UserId null - Login'e yönlendiriliyor");
                return RedirectToAction("Login", "Account");
            }

            Console.WriteLine($"Gelen blog Title: {blog.Title}");
            Console.WriteLine($"Gelen blog Description: {blog.Description}");
            
            // Manuel olarak gerekli alanları doldur
            blog.OwnerId = userId.Value;
            blog.CreatedAt = DateTime.UtcNow;
            blog.UpdatedAt = DateTime.UtcNow;
            blog.Slug = GenerateSlug(blog.Title);
            
            Console.WriteLine($"Oluşturulan Slug: {blog.Slug}");

            // ModelState'den otomatik doldurulan alanların hatalarını temizle
            ModelState.Remove("Slug");
            ModelState.Remove("Owner");
            ModelState.Remove("OwnerId");
            ModelState.Remove("CreatedAt");
            ModelState.Remove("UpdatedAt");
            
            Console.WriteLine($"ModelState Valid: {ModelState.IsValid}");
            
            if (!ModelState.IsValid)
            {
                Console.WriteLine("ModelState Hataları:");
                foreach (var error in ModelState)
                {
                    Console.WriteLine($"  {error.Key}: {string.Join(", ", error.Value.Errors.Select(e => e.ErrorMessage))}");
                }
            }

            if (ModelState.IsValid)
            {
                Console.WriteLine("ModelState geçerli - Blog kaydediliyor");
                
               
                var existingSlug = await _context.Blogs.AnyAsync(b => b.Slug == blog.Slug);
                if (existingSlug)
                {
                    blog.Slug += "-" + DateTime.UtcNow.Ticks.ToString()[^6..];
                    Console.WriteLine($"Slug benzersiz değildi, yeni Slug: {blog.Slug}");
                }

                try
                {
                    _context.Blogs.Add(blog);
                    await _context.SaveChangesAsync();
                    
                    Console.WriteLine("Blog başarıyla kaydedildi");
                    TempData["SuccessMessage"] = "Blog başarıyla oluşturuldu!";
                    return RedirectToAction("MyBlogs");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Veritabanı hatası: {ex.Message}");
                    ModelState.AddModelError("", "Blog kaydedilirken bir hata oluştu.");
                }
            }
            
            Console.WriteLine("ModelState geçersiz - Form tekrar gösteriliyor");
            return View(blog);
        }

        // GET: Blog/Details/{slug} - Blog detaylarını göster
        public async Task<IActionResult> Details(string slug)
        {
            Console.WriteLine($"=== Blog Details - Slug: {slug} ===");
            
            if (string.IsNullOrEmpty(slug))
            {
                return NotFound();
            }

            var blog = await _context.Blogs
                .Include(b => b.Owner)
                .Include(b => b.BlogEntries.OrderByDescending(e => e.CreatedAt))
                .FirstOrDefaultAsync(b => b.Slug == slug);

            if (blog == null)
            {
                return NotFound();
            }

            // Görüntüleme sayısını artır (sadece blog sahibi değilse)
            var currentUserId = HttpContext.Session.GetInt32("UserId");
            if (currentUserId != blog.OwnerId)
            {
                blog.ViewCount++;
                await _context.SaveChangesAsync();
                Console.WriteLine($"Blog görüntüleme sayısı artırıldı: {blog.ViewCount}");
            }

            return View(blog);
        }

        // GET: Blog/Edit/{id} - Blog düzenleme formu
        public async Task<IActionResult> Edit(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var blog = await _context.Blogs.FindAsync(id);
            if (blog == null)
            {
                return NotFound();
            }

            // Sadece blog sahibi düzenleyebilir
            if (blog.OwnerId != userId.Value)
            {
                return Forbid();
            }

            return View(blog);
        }

        // POST: Blog/Edit/{id} - Blog düzenleme
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Blog blog)
        {
            Console.WriteLine($"=== Edit POST action çağrıldı - ID: {id} ===");
            
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (id != blog.Id)
            {
                return NotFound();
            }

            var existingBlog = await _context.Blogs.FindAsync(id);
            if (existingBlog == null)
            {
                return NotFound();
            }

            // Sadece blog sahibi düzenleyebilir
            if (existingBlog.OwnerId != userId.Value)
            {
                return Forbid();
            }

            Console.WriteLine($"Gelen blog Title: {blog.Title}");
            Console.WriteLine($"Gelen blog Description: {blog.Description}");

            // ModelState'den otomatik doldurulan alanların hatalarını temizle
            ModelState.Remove("Owner");
            ModelState.Remove("OwnerId");
            ModelState.Remove("CreatedAt");
            ModelState.Remove("UpdatedAt");
            ModelState.Remove("Slug");
            ModelState.Remove("BlogEntries");

            Console.WriteLine($"ModelState Valid: {ModelState.IsValid}");
            
            if (!ModelState.IsValid)
            {
                Console.WriteLine("ModelState Hataları:");
                foreach (var error in ModelState)
                {
                    Console.WriteLine($"  {error.Key}: {string.Join(", ", error.Value.Errors.Select(e => e.ErrorMessage))}");
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    existingBlog.Title = blog.Title;
                    existingBlog.Description = blog.Description;
                    existingBlog.UpdatedAt = DateTime.UtcNow;
                    
                    // Başlık değişirse slug'ı da güncelle
                    if (existingBlog.Title != blog.Title)
                    {
                        existingBlog.Slug = GenerateSlug(blog.Title);
                        
                        // Yeni slug'ın benzersiz olduğundan emin ol
                        var existingSlug = await _context.Blogs
                            .AnyAsync(b => b.Slug == existingBlog.Slug && b.Id != id);
                        if (existingSlug)
                        {
                           existingBlog.Slug += "-" + DateTime.UtcNow.Ticks.ToString()[^6..];
                        }
                    }

                    await _context.SaveChangesAsync();
                    Console.WriteLine("Blog başarıyla güncellendi");
                    TempData["SuccessMessage"] = "Blog başarıyla güncellendi!";
                    return RedirectToAction("Details", new { slug = existingBlog.Slug });
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Veritabanı hatası: {ex.Message}");
                    ModelState.AddModelError("", "Blog güncellenirken bir hata oluştu.");
                }
            }

            Console.WriteLine("ModelState geçersiz - Form tekrar gösteriliyor");
            return View(blog);
        }

        // POST: Blog/Delete/{id} - Blog silme
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var blog = await _context.Blogs
                .Include(b => b.BlogEntries)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (blog == null)
            {
                return NotFound();
            }

            // Sadece blog sahibi silebilir
            if (blog.OwnerId != userId.Value)
            {
                return Forbid();
            }

            _context.Blogs.Remove(blog);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Blog başarıyla silindi!";
            return RedirectToAction("MyBlogs");
        }

        // Slug oluşturma helper metodu
        private static string GenerateSlug(string title)
        {
            if (string.IsNullOrEmpty(title))
                return "";

            // Türkçe karakterleri değiştir
            title = title.ToLowerInvariant();
            title = title.Replace("ç", "c")
                         .Replace("ğ", "g")
                         .Replace("ı", "i")
                         .Replace("ö", "o")
                         .Replace("ş", "s")
                         .Replace("ü", "u");

            // Özel karakterleri ve boşlukları - ile değiştir
            var sb = new StringBuilder();
            foreach (char c in title)
            {
                if (char.IsLetterOrDigit(c))
                {
                    sb.Append(c);
                }
                else if (char.IsWhiteSpace(c) || c == '-')
                {
                    sb.Append('-');
                }
            }

            string slug = sb.ToString();
            
            // Birden fazla - işaretini tekle indir
            while (slug.Contains("--"))
            {
                slug = slug.Replace("--", "-");
            }

            // Başında ve sonunda - varsa temizle
            slug = slug.Trim('-');

            return slug;
        }
    }
}