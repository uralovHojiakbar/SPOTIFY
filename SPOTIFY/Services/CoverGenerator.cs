using System.Text;

namespace SPOTIFY.Services;

public static class CoverGenerator
{
    public static string GenerateCoverDataUrl(string title, string artist, int w, int h, int seed)
    {
        // seed’dan ranglar
        var rng = new Random(seed);
        string bg1 = $"rgb({rng.Next(30, 230)},{rng.Next(30, 230)},{rng.Next(30, 230)})";
        string bg2 = $"rgb({rng.Next(30, 230)},{rng.Next(30, 230)},{rng.Next(30, 230)})";

        // matnlar (SVG safe)
        string t = Escape(title);
        string a = Escape(artist);

        string svg = $@"
<svg xmlns='http://www.w3.org/2000/svg' width='{w}' height='{h}' viewBox='0 0 {w} {h}'>
  <defs>
    <linearGradient id='g' x1='0' y1='0' x2='1' y2='1'>
      <stop offset='0' stop-color='{bg1}'/>
      <stop offset='1' stop-color='{bg2}'/>
    </linearGradient>
  </defs>
  <rect width='100%' height='100%' fill='url(#g)'/>
  <circle cx='{w * 0.75}' cy='{h * 0.25}' r='{Math.Min(w, h) * 0.18}' fill='rgba(255,255,255,0.20)'/>
  <circle cx='{w * 0.25}' cy='{h * 0.75}' r='{Math.Min(w, h) * 0.22}' fill='rgba(0,0,0,0.12)'/>

  <text x='32' y='{h - 110}' font-family='Arial, sans-serif' font-size='28' fill='white' opacity='0.95'>
    {t}
  </text>
  <text x='32' y='{h - 70}' font-family='Arial, sans-serif' font-size='20' fill='white' opacity='0.85'>
    {a}
  </text>
  <text x='32' y='{h - 35}' font-family='Arial, sans-serif' font-size='14' fill='white' opacity='0.70'>
    SPOTIFY • {seed}
  </text>
</svg>";

        string base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(svg));
        return "data:image/svg+xml;base64," + base64;
    }

    private static string Escape(string s)
        => (s ?? "").Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;");
}
