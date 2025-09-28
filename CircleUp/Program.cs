using Serilog;
using CircleUp.Data;
using CircleUp.Data.Helpers;
using CircleUp.Data.Services;
using Microsoft.EntityFrameworkCore;
using CircleUp.Middlewares;
using Microsoft.AspNetCore.Identity;
using CircleUp.Data.Models;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// ✅ Setup Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.File("Logs/app_log.txt", rollingInterval: RollingInterval.Day)
    .WriteTo.Console()
    .Enrich.FromLogContext()
    .MinimumLevel.Error()
    .CreateLogger();

builder.Host.UseSerilog();

// ✅ Add MVC
builder.Services.AddControllersWithViews();

// ✅ Database
var dbConnectionString = builder.Configuration.GetConnectionString("Default");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(dbConnectionString));

// ✅ App services
builder.Services.AddScoped<IPostsService, PostsService>();
builder.Services.AddScoped<IHashtagsService, HashtagsService>();
builder.Services.AddScoped<IStoriesServices, StoriesService>();
builder.Services.AddScoped<IFilesService, FilesService>();
builder.Services.AddScoped<IUsersService, UsersService>();

// ✅ Identity
builder.Services.AddIdentity<User, IdentityRole<int>>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Authentication/Login";
    options.AccessDeniedPath = "/Authentication/AccessDenied";

    options.Cookie.IsEssential = true;
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;

    // ✅ Session-only cookie (removed on browser close)
    options.Cookie.MaxAge = null;
    options.SlidingExpiration = false;
    // DO NOT set ExpireTimeSpan = TimeSpan.Zero (causes instant logout)
});

//// ✅ Authentication (explicit cookie setup)
//builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
//    .AddCookie(options =>
//    {
//        options.LoginPath = "/Authentication/Login";
//        options.AccessDeniedPath = "/Authentication/AccessDenied";
//        options.SlidingExpiration = true;
//    });

// ✅ Authorization
builder.Services.AddAuthorization();

var app = builder.Build();

// ✅ Environment check
Console.WriteLine($"Current environment: {app.Environment.EnvironmentName}");

// ✅ Seed database
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await dbContext.Database.MigrateAsync();
    await DbInitializer.SeedAsync(dbContext);

    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<int>>>();
    await DbInitializer.SeedUsersAndRolesAsync(userManager, roleManager);
}

// ✅ Global exception handler
app.UseGlobalExceptionHandler();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// ✅ Must be in this order
app.UseAuthentication();
app.UseAuthorization();

// ✅ Default route = Login page
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Authentication}/{action=Login}/{id?}");

app.Run();
