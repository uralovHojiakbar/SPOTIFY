using SPOTIFY.Models;

namespace SPOTIFY.Services;

public class SongGenerator
{
    public async Task<List<SongDto>> GeneratePageAsync(
    Dictionary<string, List<string>> loc,
    string locale,
    ulong userSeed,
    int page,
    int pageSize,
    double avgLikes)
    {
        ulong combined = SeedUtil.Combine(userSeed, page);
        int contentSeed = SeedUtil.StableIntSeed(combined);

        int total = AudioLibrary.TrackCount; 

       
        int startIndex = (page - 1) * pageSize + 1;

        if (startIndex > total)
            return new List<SongDto>();

        int count = Math.Min(pageSize, total - startIndex + 1);

        var list = new List<SongDto>(count);

        for (int i = 0; i < count; i++)
        {
            int index = startIndex + i;

            var recordRng = new Random(unchecked(contentSeed ^ index * 104729));

            string title = $"{Pick(loc, "adjective", recordRng)} {Pick(loc, "noun", recordRng)}";
            string artist = GenerateArtist(loc, recordRng);
            string album = recordRng.NextDouble() < 0.25 ? "Single"
                : $"{Pick(loc, "noun", recordRng)} {Pick(loc, "noun", recordRng)}";
            string genre = Pick(loc, "genre", recordRng);

            int likes = LikesCalculator.ComputeLikes(avgLikes, index, combined);

            string cover = CoverGenerator.GenerateCoverDataUrl(
                title, artist, 420, 420, unchecked(contentSeed + index));

            string review = GenerateReview(locale, title, artist, recordRng);
            string label = GenerateLabel(recordRng);
            int year = recordRng.Next(2012, 2026);
            int durationSec = recordRng.Next(95, 240);
            string lyrics = GenerateLyrics(title, artist, recordRng);

            list.Add(new SongDto
            {
                Index = index,
                Title = title,
                Artist = artist,
                Album = album,
                Genre = genre,
                Likes = likes,
                CoverDataUrl = cover,
                Review = review,
                AudioUrl = AudioLibrary.GetAudioUrlForIndex(index),
                Label = label,
                Year = year,
                DurationSec = durationSec,
                Lyrics = lyrics
            });
        }

        return list;
    }


    private static string Pick(Dictionary<string, List<string>> loc, string kind, Random rng)
    {
        if (!loc.TryGetValue(kind, out var arr) || arr.Count == 0)
            return "Unknown";
        return arr[rng.Next(arr.Count)];
    }

    private static string GenerateArtist(Dictionary<string, List<string>> loc, Random rng)
    {
        bool band = rng.NextDouble() < 0.45;
        if (band)
            return $"{Pick(loc, "adjective", rng)} {Pick(loc, "bandWord", rng)}";

        return $"{Pick(loc, "firstName", rng)} {Pick(loc, "lastName", rng)}";
    }

    private static string GenerateReview(string locale, string title, string artist, Random rng)
    {
        string[] templates =
        [
            $"“{title}” by {artist} feels surprisingly catchy, with a clean hook and a steady groove.",
            $"The track “{title}” has a warm vibe and a memorable chorus — easy to replay.",
            $"A playful arrangement and a crisp rhythm make “{title}” stand out in a fun way.",
            $"“{title}” balances mellow moments with energetic peaks — a solid short preview."
        ];
        return templates[rng.Next(templates.Length)];
    }

    private static string GenerateLabel(Random rng)
    {
        string[] labels =
        [
            "Apple Records", "Moonlight Music", "Indie Pulse", "City Sounds",
            "Neon Tape", "Bluebird Records", "Studio North", "Golden Gate Records"
        ];
        return labels[rng.Next(labels.Length)];
    }

    private static string GenerateLyrics(string title, string artist, Random rng)
    {
        string[] a =
        [
            "Every beat reminds me of you, tearing me apart",
            "In the million suns that shine, you're the brightest star",
            "At the break of dawn, you're all I want, no matter how far",
            "I keep the radio low, but the feelings stay loud"
        ];

        string[] b =
        [
            "Oh, I try to move on",
            "But the night keeps pulling me back",
            "I run through empty streets",
            "Chasing echoes in the dark"
        ];

        int total = rng.Next(10, 17);
        var lines = new List<string>();

        for (int i = 0; i < total; i++)
        {
            var pick = (i % 2 == 0) ? a : b;
            lines.Add(pick[rng.Next(pick.Length)]);
        }

        return string.Join("\n", lines);
    }
}
