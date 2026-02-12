using System.Text.Json;

namespace SPOTIFY.Services;

public class LocaleProvider
{
    // wwwroot/locales/en-US.json, de-DE.json
    private readonly string _localesDir;

    public LocaleProvider(IWebHostEnvironment env)
    {
        _localesDir = Path.Combine(env.WebRootPath, "locales");
    }

    public async Task<Dictionary<string, List<string>>> GetLocaleAsync(string locale)
    {
        locale = string.IsNullOrWhiteSpace(locale) ? "en-US" : locale;

        // fallback: en-US
        var path = Path.Combine(_localesDir, $"{locale}.json");
        if (!File.Exists(path))
            path = Path.Combine(_localesDir, "en-US.json");

        var json = await File.ReadAllTextAsync(path);
        var dict = JsonSerializer.Deserialize<Dictionary<string, List<string>>>(json);

        return dict ?? new Dictionary<string, List<string>>();
    }
}
