using Microsoft.EntityFrameworkCore;
using SPOTIFY.Data;
using SPOTIFY.Services;

var builder = WebApplication.CreateBuilder(args);


var port = Environment.GetEnvironmentVariable("PORT") ?? "10000";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

builder.Services.AddControllersWithViews();

var pgCs = builder.Configuration.GetConnectionString("Pg");
if (string.IsNullOrWhiteSpace(pgCs))
{
    throw new Exception(
        "Missing connection string 'ConnectionStrings:Pg'. " +
        "On Render set Environment Variable: ConnectionStrings__Pg"
    );
}

builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseNpgsql(pgCs,
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

app.MapControllers();


app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
