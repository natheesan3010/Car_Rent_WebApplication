using CloudinaryDotNet;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using QuickRentMyRide.Data;
using QuickRentMyRide.Models;
using Stripe;

// Alias to avoid conflict between CloudinaryDotNet.Account and Stripe.Account
using CloudinaryAccount = CloudinaryDotNet.Account;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

// ---------------- Add services ----------------
builder.Services.AddControllersWithViews();

// ---------------- DbContext ----------------
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(configuration.GetConnectionString("QuickRentMyRide"))
);

// ---------------- Authentication ----------------
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.ExpireTimeSpan = TimeSpan.FromHours(1);
    });

// ---------------- Stripe ----------------
StripeConfiguration.ApiKey = configuration["Stripe:SecretKey"];

// ---------------- Cloudinary Settings ----------------
builder.Services.Configure<CloudinarySettings>(
    configuration.GetSection("CloudinarySettings")
);

builder.Services.AddSingleton(provider =>
{
    var config = provider.GetRequiredService<IOptions<CloudinarySettings>>().Value;
    var account = new CloudinaryAccount(config.CloudName, config.ApiKey, config.ApiSecret);
    return new Cloudinary(account);
});

// ---------------- Session ----------------
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// ---------------- Seed Default Admin ----------------
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var hasher = new PasswordHasher<User>();

    string adminEmail = "admin@gmail.com";   // ✅ உங்கள் default admin email
    string adminUserName = "Admin";          // ✅ username
    string adminPassword = "Admin@123";      // ✅ default password (later change!)

    if (!db.Users.Any(u => u.Gmail_Address == adminEmail))
    {
        var adminUser = new User
        {
            Gmail_Address = adminEmail,
            Username = adminUserName,
            Role = "Admin"
        };
        adminUser.PasswordHash = hasher.HashPassword(adminUser, adminPassword);

        db.Users.Add(adminUser);
        db.SaveChanges();
    }
}

// ---------------- Middleware ----------------
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();
app.UseSession();

// ---------------- Default Route ----------------
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);

app.Run();
