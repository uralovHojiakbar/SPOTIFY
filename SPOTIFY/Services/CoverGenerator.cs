using SkiaSharp;

namespace SPOTIFY.Services;

public static class CoverGenerator
{
    public static string GenerateCoverDataUrl(string title, string artist, int w, int h, int seed)
    {
        using var bmp = new SKBitmap(w, h);
        using var canvas = new SKCanvas(bmp);

        var rng = new Random(seed);

      
        canvas.Clear(new SKColor((byte)rng.Next(10, 245), (byte)rng.Next(10, 245), (byte)rng.Next(10, 245)));
        using var paint = new SKPaint { IsAntialias = true };

        for (int i = 0; i < 12; i++)
        {
            paint.Color = new SKColor((byte)rng.Next(10, 245), (byte)rng.Next(10, 245), (byte)rng.Next(10, 245), (byte)rng.Next(80, 160));
            var x = rng.Next(-50, w);
            var y = rng.Next(-50, h);
            var rw = rng.Next(60, 220);
            var rh = rng.Next(60, 220);
            canvas.DrawRoundRect(new SKRect(x, y, x + rw, y + rh), 18, 18, paint);
        }

   
        using var textPaint = new SKPaint
        {
            Color = SKColors.White,
            IsAntialias = true,
            TextSize = 34,
            Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyle.Bold)
        };

  
        var titleLines = SplitToLines(title, 18);
        float ty = 70;
        foreach (var line in titleLines)
        {
            canvas.DrawText(line, 30, ty, textPaint);
            ty += 42;
        }

        textPaint.TextSize = 24;
        textPaint.Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyle.Normal);
        canvas.DrawText(artist, 30, h - 40, textPaint);

        using var image = SKImage.FromBitmap(bmp);
        using var data = image.Encode(SKEncodedImageFormat.Png, 90);
        var b64 = Convert.ToBase64String(data.ToArray());
        return $"data:image/png;base64,{b64}";
    }

    private static IEnumerable<string> SplitToLines(string text, int maxLen)
    {
        if (string.IsNullOrWhiteSpace(text)) return new[] { "" };

        var words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var lines = new List<string>();
        var cur = "";

        foreach (var w in words)
        {
            var candidate = string.IsNullOrEmpty(cur) ? w : cur + " " + w;
            if (candidate.Length > maxLen)
            {
                lines.Add(cur);
                cur = w;
            }
            else cur = candidate;
        }

        if (!string.IsNullOrEmpty(cur)) lines.Add(cur);
        return lines;
    }
}

