namespace SPOTIFY.Services;

public static class AudioLibrary
{
    public const int TrackCount = 20; 

    public static string GetAudioUrlForIndex(int index)
    {
        if (index < 1) index = 1;
        int n = ((index - 1) % TrackCount) + 1;
        string file = n.ToString("D3") + ".mp3";
        return "/audio/" + file;
    }
}
