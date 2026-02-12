namespace SPOTIFY.Models;

public class SongDto
{
    public int Index { get; set; }           
    public string Title { get; set; } = "";
    public string Artist { get; set; } = "";
    public string Album { get; set; } = "";
    public string Genre { get; set; } = "";
    public int Likes { get; set; }


    public string CoverDataUrl { get; set; } = "";
    public string Review { get; set; } = "";
    public string PreviewUrl { get; set; } = "";
    public string AudioUrl { get; set; } = "";
  
    public string Label { get; set; } = "";
    public int Year { get; set; }
    public int DurationSec { get; set; }

    public string Lyrics { get; set; } = "";
 

}
