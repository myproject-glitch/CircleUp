using Serilog;
using CircleUp.Data;
using CircleUp.Data.Helpers;
using CircleUp.Data.Services;
using Microsoft.EntityFrameworkCore;
using CircleUp.Middlewares; // ✅ add this namespace

var builder = WebApplication.CreateBuilder(args);

// ✅ Setup Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.File("Logs/app_log.txt", rollingInterval: RollingInterval.Day)
    .WriteTo.Console()
    .Enrich.FromLogContext()
    .MinimumLevel.Error()
    .CreateLogger();

builder.Host.UseSerilog();

// Add services
builder.Services.AddControllersWithViews();
var dbConnectionString = builder.Configuration.GetConnectionString("Default");
builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(dbConnectionString));

builder.Services.AddScoped<IPostsService, PostsService>();
builder.Services.AddScoped<IHashtagsService, HashtagsService>();
builder.Services.AddScoped<IStoriesServices, StoriesService>();
builder.Services.AddScoped<IFilesService, FilesService>();
builder.Services.AddScoped<IUsersService, UsersService>();

var app = builder.Build();

Console.WriteLine($"Current environment: {app.Environment.EnvironmentName}");

// ✅ Seed database
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await dbContext.Database.MigrateAsync();
    await DbInitializer.SeedAsync(dbContext);
}

// ✅ Use global exception middleware
app.UseGlobalExceptionHandler();

// ✅ Use HSTS only outside development
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
