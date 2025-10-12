using UnityEngine;

namespace AudioAnalysis
{
    public class FrequencyRange
    {
        public readonly int StartIndex;
        public readonly int EndIndex;

        public readonly float StartFrequency;
        public readonly float EndFrequency;

        public FrequencyRange(float from, float to)
        {
            StartFrequency = from;
            EndFrequency = to;

            StartIndex = GetSampleIndex(from);
            EndIndex = GetSampleIndex(to);
        }

        private static int GetSampleIndex(float frequency)
        {
            float hzPerSample = FFTAudioAnalysisSettings.SampleRate / (float)FFTAudioAnalysisSettings.NumSamples;
            int index = Mathf.FloorToInt(frequency / hzPerSample);
            return Mathf.Clamp(index, 0, FFTAudioAnalysisSettings.NumSamples - 1);
        }
    }
}