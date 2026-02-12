using System.Text.Json;

namespace SPOTIFY.Services;

public class LocaleProvider
{
    private readonly string _localesDir;

    public LocaleProvider(IWebHostEnvironment env)
    {
        _localesDir = Path.Combine(env.WebRootPath ?? "wwwroot", "locales");
    }

    public async Task<Dictionary<string, List<string>>> GetLocaleAsync(string locale)
    {
        locale = string.IsNullOrWhiteSpace(locale) ? "en-US" : locale;

        var preferred = Path.Combine(_localesDir, $"{locale}.json");
        var fallback = Path.Combine(_localesDir, "en-US.json");

        string? json = null;

        if (File.Exists(preferred))
            json = await File.ReadAllTextAsync(preferred);
        else if (File.Exists(fallback))
            json = await File.ReadAllTextAsync(fallback);

        if (string.IsNullOrWhiteSpace(json))
        {
            // Locale fayllar topilmadi -> API yiqilmasin
            return new Dictionary<string, List<string>>();
        }

        try
        {
            var dict = JsonSerializer.Deserialize<Dictionary<string, List<string>>>(json);
            return dict ?? new Dictionary<string, List<string>>();
        }
        catch
        {
            // JSON buzilgan bo‘lsa ham API yiqilmasin
            return new Dictionary<string, List<string>>();
        }
    }
}
