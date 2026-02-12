using Microsoft.AspNetCore.Mvc;
using SPOTIFY.Models;
using SPOTIFY.Services;

namespace SPOTIFY.Controllers;

[ApiController]
[Route("api/songs")]
public class ApiSongsController : ControllerBase
{
    private readonly LocaleProvider _localeProvider;
    private readonly SongGenerator _generator;
    private readonly IWebHostEnvironment _env;

    public ApiSongsController(LocaleProvider localeProvider, SongGenerator generator, IWebHostEnvironment env)
    {
        _localeProvider = localeProvider;
        _generator = generator;
        _env = env;
    }

    [HttpGet("page")]
    public async Task<ActionResult<PageResponse>> GetPage(
        [FromQuery] string locale = "en-US",
        [FromQuery] ulong seed = 1,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] double avgLikes = 3.7)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 5, 50);
        avgLikes = Math.Clamp(avgLikes, 0, 10);

        var loc = await _localeProvider.GetLocaleAsync(locale);
        var items = await _generator.GeneratePageAsync(loc, locale, seed, page, pageSize, avgLikes);

        return Ok(new PageResponse
        {
            Page = page,
            PageSize = pageSize,
            Locale = locale,
            Seed = seed,
            AvgLikes = avgLikes,
            Items = items
        });
    }

    [HttpGet("preview")]
    public IActionResult Preview(
        [FromQuery] ulong seed = 1,
        [FromQuery] int index = 1,
        [FromQuery] int seconds = 15)
    {
        index = Math.Max(1, index);
        seconds = Math.Clamp(seconds, 6, 30);

        var audioDir = Path.Combine(_env.WebRootPath, "audio");

        if (Directory.Exists(audioDir))
        {
            var files = Directory.EnumerateFiles(audioDir, "*.*", SearchOption.TopDirectoryOnly)
                .Where(f => f.EndsWith(".mp3", StringComparison.OrdinalIgnoreCase) ||
                            f.EndsWith(".wav", StringComparison.OrdinalIgnoreCase))
                .OrderBy(f => f)
                .ToList();

            if (files.Count > 0)
            {
                int i = (index - 1) % files.Count;
                string filePath = files[i];

                string contentType = filePath.EndsWith(".mp3", StringComparison.OrdinalIgnoreCase)
                    ? "audio/mpeg"
                    : "audio/wav";

                return PhysicalFile(filePath, contentType);
            }
        }

        // fallback: generatsiya WAV
        ulong combined = seed ^ (ulong)index * 11400714819323198485UL;
        int audioSeed = unchecked((int)(combined ^ (combined >> 32)));
        var wav = AudioPreviewGenerator.GenerateWav(audioSeed, seconds: seconds);
        return File(wav, "audio/wav");
    }
}
