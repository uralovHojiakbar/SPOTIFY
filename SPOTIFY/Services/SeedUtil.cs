namespace SPOTIFY.Services;

public static class SeedUtil
{
    
    public static ulong Combine(ulong userSeed, int page)
    {
        unchecked
        {
            const ulong A = 6364136223846793005UL;
            const ulong B = 1442695040888963407UL;
            return (userSeed * A) + ((ulong)page * B) + 0x9E3779B97F4A7C15UL;
        }
    }

    public static int StableIntSeed(ulong u)
    {
        unchecked
        {
            return (int)(u ^ (u >> 32));
        }
    }
}
