using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BlogProject.Data;
using BlogProject.Models;
using Markdig;

namespace BlogProject.Controllers
{
    public class CommentController : Controller
    {
        private readonly BlogDbContext _context;
        private readonly MarkdownPipeline _markdownPipeline;

        public CommentController(BlogDbContext context)
        {
            _context = context;
            _markdownPipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
        }

        // Yorum ekleme
        [HttpPost]
        public async Task<IActionResult> Add(int blogEntryId, int? parentCommentId, string content, string? guestName = null, string? guestEmail = null)
        {
            var isLoggedIn = IsUserLoggedIn();
            
            // İçerik kontrolü
            if (string.IsNullOrWhiteSpace(content))
            {
                TempData["ErrorMessage"] = "Yorum içeriği boş olamaz.";
                return RedirectToAction("Details", "BlogEntry", new { id = blogEntryId });
            }

            // Misafir kullanıcı için ad ve email kontrolü
            if (!isLoggedIn)
            {
                if (string.IsNullOrWhiteSpace(guestName))
                {
                    TempData["ErrorMessage"] = "Misafir kullanıcılar için ad gereklidir.";
                    return RedirectToAction("Details", "BlogEntry", new { id = blogEntryId });
                }
                
                if (string.IsNullOrWhiteSpace(guestEmail) || !IsValidEmail(guestEmail))
                {
                    TempData["ErrorMessage"] = "Geçerli bir email adresi gereklidir.";
                    return RedirectToAction("Details", "BlogEntry", new { id = blogEntryId });
                }
            }

            var blogEntry = await _context.BlogEntries.FindAsync(blogEntryId);
            if (blogEntry == null)
            {
                return NotFound();
            }

            if (!blogEntry.CommentsEnabled)
            {
                TempData["ErrorMessage"] = "Bu yazıya yorum yapma özelliği kapatılmış.";
                return RedirectToAction("Details", "BlogEntry", new { id = blogEntryId });
            }

            // Parent comment kontrolü
            if (parentCommentId.HasValue)
            {
                var parentComment = await _context.Comments.FindAsync(parentCommentId.Value);
                if (parentComment == null || parentComment.BlogEntryId != blogEntryId)
                {
                    TempData["ErrorMessage"] = "Geçersiz ana yorum.";
                    return RedirectToAction("Details", "BlogEntry", new { id = blogEntryId });
                }
            }

            var comment = new Comment
            {
                BlogEntryId = blogEntryId,
                ParentCommentId = parentCommentId,
                UserId = isLoggedIn ? GetCurrentUserId() : null,
                GuestName = !isLoggedIn ? guestName?.Trim() : null,
                GuestEmail = !isLoggedIn ? guestEmail?.Trim() : null,
                Content = content.Trim(),

                CreatedAt = DateTime.UtcNow  

            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Yorumunuz başarıyla eklendi.";
            return RedirectToAction("Details", "BlogEntry", new { id = blogEntryId });
        }

        // Yorum düzenleme formu
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var comment = await _context.Comments
                .Include(c => c.BlogEntry)
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (comment == null)
            {
                return NotFound();
            }

            // Sadece yorum sahibi düzenleyebilir (misafir kullanıcılar düzenleyemez)
            if (!comment.UserId.HasValue || comment.UserId != userId)
            {
                TempData["ErrorMessage"] = "Bu yorumu düzenleme yetkiniz yok.";
                return RedirectToAction("Details", "BlogEntry", new { id = comment.BlogEntryId });
            }

            return View(comment);
        }

        // Yorum düzenleme
        [HttpPost]
        public async Task<IActionResult> Edit(int id, string content)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (string.IsNullOrWhiteSpace(content))
            {
                TempData["ErrorMessage"] = "Yorum içeriği boş olamaz.";
                return RedirectToAction("Edit", new { id });
            }

            var comment = await _context.Comments.FindAsync(id);
            if (comment == null)
            {
                return NotFound();
            }

            if (!comment.UserId.HasValue || comment.UserId != userId)
            {
                TempData["ErrorMessage"] = "Bu yorumu düzenleme yetkiniz yok.";
                return RedirectToAction("Details", "BlogEntry", new { id = comment.BlogEntryId });
            }

            comment.Content = content.Trim();

            comment.UpdatedAt = DateTime.UtcNow; 


            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Yorumunuz başarıyla güncellendi.";
            TempData.Keep("SuccessMessage"); 
            return RedirectToAction("Details", "BlogEntry", new { id = comment.BlogEntryId });
        }

        // Yorum silme
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var comment = await _context.Comments
                .Include(c => c.BlogEntry)
                .ThenInclude(b => b.Blog)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (comment == null)
            {
                return NotFound();
            }

            var blogEntry = comment.BlogEntry;

            // Yorum sahibi (sadece kayıtlı kullanıcılar) veya blog sahibi silebilir
            if ((!comment.UserId.HasValue || comment.UserId != userId) && 
                blogEntry?.Blog?.OwnerId != userId)
            {
                TempData["ErrorMessage"] = "Bu yorumu silme yetkiniz yok.";
                return RedirectToAction("Details", "BlogEntry", new { id = comment.BlogEntryId });
            }

            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Yorum başarıyla silindi.";
            return RedirectToAction("Details", "BlogEntry", new { id = comment.BlogEntryId });
        }

        // Yardımcı metodlar
        private bool IsUserLoggedIn()
        {
            return HttpContext.Session.GetInt32("UserId").HasValue;
        }

        private int? GetCurrentUserId()
        {
            return HttpContext.Session.GetInt32("UserId");
        }

        // Email validasyon metodu
        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }
}