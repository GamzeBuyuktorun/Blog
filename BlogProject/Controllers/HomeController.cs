using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using BlogProject.Models;



namespace BlogProject.Controllers
{
    public class HomeController : Controller
    {
        // Ana sayfa istekleri Blog/Index'e yönlendirilir
        public IActionResult Index()
        {
            Console.WriteLine("=== HomeController Index - Blog/Index'e yönlendiriliyor ===");
            return RedirectToAction("Index", "Blog");
        }

        // Hata sayfası
        public IActionResult Error()
        {
            return View();
        }
    }
}