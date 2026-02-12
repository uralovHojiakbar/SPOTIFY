namespace SPOTIFY.Models;

public class PageResponse
{
    public int Page { get; set; }
    public int PageSize { get; set; }
    public string Locale { get; set; } = "";
    public ulong Seed { get; set; }
    public double AvgLikes { get; set; }
    public List<SongDto> Items { get; set; } = new();
}
