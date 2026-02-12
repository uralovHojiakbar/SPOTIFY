namespace SPOTIFY.Services;

public static class LikesCalculator
{
    public static int ComputeLikes(double avgLikes, int recordIndex, ulong baseSeed)
    {
        if (avgLikes <= 0) return 0;
        if (avgLikes >= 10) return 10;

        int floor = (int)Math.Floor(avgLikes);
        double frac = avgLikes - floor;

        var rng = new Random(unchecked((int)(baseSeed ^ (ulong)recordIndex * 2654435761UL)));

        if (frac <= 0) return floor;

        return rng.NextDouble() < frac ? floor + 1 : floor;
    }
}
