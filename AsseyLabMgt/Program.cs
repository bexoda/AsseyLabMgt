using AsseyLabMgt.Data;
using AsseyLabMgt.Models;
using AsseyLabMgt.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Infrastructure;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connectionString));
//builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<AppUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = true;

    // Relaxing the password strength requirements:
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 6;  // You can reduce the required length, the default is usually 6
    options.Password.RequiredUniqueChars = 1; // You can reduce the number of required unique characters
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddScoped<StatisticsService>();
builder.Services.AddScoped<ReportService>();

builder.Services.AddControllersWithViews();

builder.Services.AddAuthentication();
builder.Services.AddAuthorization();

builder.Services.AddDistributedMemoryCache(); // Adds a default in-memory implementation of IDistributedCache
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // You can set a different timeout value
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true; // Make the session cookie essential
});
// Set the QuestPDF license type before building the app
QuestPDF.Settings.License = LicenseType.Community;

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseSession(); // Add this before UseRouting()
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapControllerRoute(
        name: "report",
        pattern: "report/{action=SelectDepartment}",
        defaults: new { controller = "Report" });
app.MapControllerRoute(
        name: "generateReport",
        pattern: "report/{action=GenerateReport}/{departmentId?}",
        defaults: new { controller = "Report" });

app.MapRazorPages();

app.Run();
