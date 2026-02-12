using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SPOTIFY.Data;
using SPOTIFY.Models;

namespace SPOTIFY.Services;

public class LocaleProvider
{
    private readonly AppDbContext _db;
    private readonly IWebHostEnvironment _env;

    public LocaleProvider(AppDbContext db, IWebHostEnvironment env)
    {
        _db = db;
        _env = env;
    }

    public async Task EnsureSeededAsync()
    {
        if (await _db.LocaleEntries.AnyAsync())
            return;

        var localesPath = Path.Combine(_env.WebRootPath, "locales");
        foreach (var file in Directory.GetFiles(localesPath, "*.json"))
        {
            var locale = Path.GetFileNameWithoutExtension(file);
            var json = await File.ReadAllTextAsync(file);
            var dict = JsonSerializer.Deserialize<Dictionary<string, string[]>>(json) ?? new();

            foreach (var (kind, arr) in dict)
            {
                foreach (var val in arr)
                {
                    _db.LocaleEntries.Add(new LocaleEntry
                    {
                        Locale = locale,
                        Kind = kind,
                        Value = val
                    });
                }
            }
        }

        await _db.SaveChangesAsync();
    }

    public async Task<Dictionary<string, List<string>>> GetLocaleAsync(string locale)
    {
        var items = await _db.LocaleEntries
            .Where(x => x.Locale == locale)
            .ToListAsync();

        return items
            .GroupBy(x => x.Kind)
            .ToDictionary(g => g.Key, g => g.Select(x => x.Value).ToList());
    }
}
