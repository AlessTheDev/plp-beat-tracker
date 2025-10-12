using AudioAnalysis;

namespace Visualizers
{
    /// <summary>
    /// Used for personal visualization tests, not for the PLP algorithm
    /// </summary>
    public static class CommonFreqRange
    {
        public static readonly FrequencyRange SubBass = new(20, 100);
        public static readonly FrequencyRange Bass = new(60, 250);
        public static readonly FrequencyRange LowMid = new(250, 500);
        public static readonly FrequencyRange Midrange = new(500, 2000);
        public static readonly FrequencyRange UpperMid = new(2000, 4000);
        public static readonly FrequencyRange Presence = new(4000, 6000);
        public static readonly FrequencyRange Brilliance = new(6000, 20000);

        public static readonly FrequencyRange Snare = new(300, 5000);
        public static readonly FrequencyRange Piano = new(30, 4000);
        public static readonly FrequencyRange Vocals = new(80, 1100);
    }
}