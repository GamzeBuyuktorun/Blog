using BlogProject.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// DbContext
builder.Services.AddDbContext<BlogDbContext>(options =>
   options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Session (cache + session)
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddHttpContextAccessor();

// MVC
builder.Services.AddControllersWithViews();

var app = builder.Build();

// (opsiyonel) otomatik migrate
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<BlogDbContext>();
    db.Database.Migrate();
}

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

app.UseSession();

// Default route (EN ÖNCE - en önemli!)
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Blog}/{action=Index}/{id?}");

// Custom Routes (sonra)
app.MapControllerRoute(
    name: "blog-details",
    pattern: "blog/{slug}",
    defaults: new { controller = "Blog", action = "Details" });

app.MapControllerRoute(
    name: "entry-details", 
    pattern: "entry/{slug}",
    defaults: new { controller = "BlogEntry", action = "Details" });

app.MapControllerRoute(
    name: "all-blogs",
    pattern: "blogs",
    defaults: new { controller = "Blog", action = "Index" });

app.Run();