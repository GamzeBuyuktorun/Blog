using BlogProject.Data;
using BlogProject.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Veritabanı
builder.Services.AddDbContext<BlogDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Sadece session kullanılacaksa Identity'a gerek yok!
// builder.Services.AddIdentity<...>(...) kaldırıldı

// Session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Oturum süresi
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Session + Context kullanımı için gerekli
builder.Services.AddHttpContextAccessor();

// MVC
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseStaticFiles();
app.UseRouting();
app.UseSession(); // <- ÖNEMLİ

// app.UseAuthentication(); // kaldırıldı
// app.UseAuthorization();  // kaldırıldı

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
