using SPOTIFY.Services;

var builder = WebApplication.CreateBuilder(args);

// Render PORT
var port = Environment.GetEnvironmentVariable("PORT") ?? "10000";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

builder.Services.AddControllersWithViews();

// ✅ DB yo‘q: Locale JSON’dan o‘qiydi
builder.Services.AddSingleton<LocaleProvider>();

// generator
builder.Services.AddSingleton<SongGenerator>();

var app = builder.Build();

app.UseStaticFiles();
app.UseRouting();

app.MapControllers();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
