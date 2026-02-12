namespace SPOTIFY.Services;

public static class AudioPreviewGenerator
{
  
    public static byte[] GenerateWav(int seed, int seconds = 15, int sampleRate = 22050)
    {
        var rng = new Random(seed);

        int totalSamples = seconds * sampleRate;
        short[] samples = new short[totalSamples];

       
        int[] scale = { 0, 2, 3, 5, 7, 8, 10, 12 };

      
        int[] prog = { 0, 5, 2, 6 };

       
        int bpm = 110 + rng.Next(-8, 9);
        double secPerBeat = 60.0 / bpm;

        int samplePerBeat = (int)(sampleRate * secPerBeat);
        int stepsPerBar = 16; 
        int stepSamples = Math.Max(1, samplePerBeat / 4);

 
        double leadPhase = 0;
        double bassPhase = 0;
        double noisePhase = 0;

       
        double leadVibratoHz = 4.5 + rng.NextDouble() * 2.5;
        double leadVibratoDepth = 0.003 + rng.NextDouble() * 0.007;

       
        static double FreqFromA3(int semitoneOffset)
            => 220.0 * Math.Pow(2.0, semitoneOffset / 12.0);

      
        int[] motif = rng.Next(4) switch
        {
            0 => new[] { 0, 2, 4, 2, 0, 2, 4, 6 },
            1 => new[] { 0, 3, 2, 3, 5, 3, 2, 0 },
            2 => new[] { 4, 2, 0, 2, 4, 5, 7, 5 },
            _ => new[] { 0, 2, 0, 5, 4, 2, 0, -1 }
        };

     
        int totalSteps = (int)Math.Ceiling(totalSamples / (double)stepSamples);

   
        double restChance = 0.15 + rng.NextDouble() * 0.15;
        double syncChance = 0.10 + rng.NextDouble() * 0.15;

        for (int step = 0; step < totalSteps; step++)
        {
            int start = step * stepSamples;
            int end = Math.Min(totalSamples, start + stepSamples);

            int bar = step / stepsPerBar;
            int stepInBar = step % stepsPerBar;

           
            int chordRootScaleIdx = prog[bar % prog.Length];
            int chordRootSemi = scale[chordRootScaleIdx]; 

          
            int thirdSemi = scale[(chordRootScaleIdx + 2) % 7];
            int fifthSemi = scale[(chordRootScaleIdx + 4) % 7];

            
            double bassFreq = FreqFromA3(chordRootSemi - 12);

         
            bool isRest = rng.NextDouble() < restChance && (stepInBar % 4 != 0); 
            int motifVal = motif[(step / 2) % motif.Length]; 
            int leadSemiChoice;

            if (isRest)
            {
                leadSemiChoice = int.MinValue; 
            }
            else
            {
                
                if (rng.NextDouble() < 0.55)
                {
                    int pick = rng.Next(3);
                    leadSemiChoice = pick switch
                    {
                        0 => chordRootSemi,
                        1 => thirdSemi,
                        _ => fifthSemi
                    };
                }
                else
                {
                  
                    int idx = Math.Clamp((motifVal + 7) % 7, 0, 6);
                    leadSemiChoice = scale[idx] + (rng.NextDouble() < syncChance ? 12 : 0);
                }

               
                if (rng.NextDouble() < 0.18) leadSemiChoice += 12;
                if (rng.NextDouble() < 0.10) leadSemiChoice -= 12;
            }

            double leadFreq = leadSemiChoice == int.MinValue ? 0 : FreqFromA3(leadSemiChoice);

        
            int len = end - start;
            int a = Math.Max(1, len / 8);
            int d = Math.Max(1, len / 6);
            double sustain = 0.55;
            int r = Math.Max(1, len / 6);

            for (int i = start; i < end; i++)
            {
                int local = i - start;

              
                double env;
                if (local < a) env = local / (double)a;
                else if (local < a + d) env = 1.0 - (1.0 - sustain) * ((local - a) / (double)d);
                else if (local < len - r) env = sustain;
                else env = sustain * (1.0 - (local - (len - r)) / (double)r);

            
                double kick = 0;
                if (stepInBar % 4 == 0)
                {
                    double t = local / (double)len;
                   
                    kick = Math.Exp(-t * 10.0) * Math.Sin(2 * Math.PI * 55.0 * i / sampleRate) * 0.9;
                }

             
                double hat = 0;
                if (stepInBar % 2 == 1)
                {
                    double t = local / (double)len;
                
                    double n = (rng.NextDouble() * 2.0 - 1.0);
                    hat = Math.Exp(-t * 16.0) * n * 0.25;
                }

             
                bassPhase += 2 * Math.PI * bassFreq / sampleRate;
                double bass = Math.Sin(bassPhase) * 0.35;

            
                double lead = 0;
                if (leadFreq > 0)
                {
                    double tSec = i / (double)sampleRate;
                    double vib = Math.Sin(2 * Math.PI * leadVibratoHz * tSec) * leadVibratoDepth;
                    leadPhase += 2 * Math.PI * (leadFreq * (1.0 + vib)) / sampleRate;

              
                    lead = (Math.Sin(leadPhase) + 0.25 * Math.Sin(2 * leadPhase)) * 0.55;
                    lead *= env;
                }

             
                double v = 0.55 * lead + 0.30 * bass + 0.28 * kick + hat;

             
                v = Math.Tanh(v);

                samples[i] = (short)(v * short.MaxValue);
            }
        }

        return BuildWav(samples, sampleRate);
    }

    private static byte[] BuildWav(short[] pcmSamples, int sampleRate)
    {
        int byteRate = sampleRate * 2; 
        int dataSize = pcmSamples.Length * 2;
        int fileSize = 44 + dataSize;

        using var ms = new MemoryStream(fileSize);
        using var bw = new BinaryWriter(ms);

        bw.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));
        bw.Write(fileSize - 8);
        bw.Write(System.Text.Encoding.ASCII.GetBytes("WAVE"));

        bw.Write(System.Text.Encoding.ASCII.GetBytes("fmt "));
        bw.Write(16);
        bw.Write((short)1);
        bw.Write((short)1);
        bw.Write(sampleRate);
        bw.Write(byteRate);
        bw.Write((short)2);
        bw.Write((short)16);

        bw.Write(System.Text.Encoding.ASCII.GetBytes("data"));
        bw.Write(dataSize);

        foreach (var s in pcmSamples)
            bw.Write(s);

        bw.Flush();
        return ms.ToArray();
    }
}
