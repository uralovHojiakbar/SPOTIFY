using Microsoft.EntityFrameworkCore;
using SPOTIFY.Data;
using SPOTIFY.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseNpgsql(builder.Configuration.GetConnectionString("Pg"),
        b => b.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)
              .MigrationsHistoryTable("__efmigrations_history", "public")
    )
);


builder.Services.AddScoped<LocaleProvider>();
builder.Services.AddSingleton<SongGenerator>();

var app = builder.Build();


using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();

    var loc = scope.ServiceProvider.GetRequiredService<LocaleProvider>();
    await loc.EnsureSeededAsync();
}


app.UseStaticFiles();

app.UseRouting();

var port = Environment.GetEnvironmentVariable("PORT") ?? "10000";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

app.MapControllers();


app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
