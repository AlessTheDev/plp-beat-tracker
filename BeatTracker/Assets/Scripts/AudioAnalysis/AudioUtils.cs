using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AudioAnalysis
{
    public static class AudioUtils
    {
        /// <summary>
        /// Local maxima by simple comparison of neighboring values like scypy
        /// /// <param name="signal">
        /// The input signal array in which to detect peaks.
        /// </param>
        /// <param name="minHeight">
        /// Minimum amplitude a sample must have to be considered a peak.  
        /// </param>
        /// <param name="minDistance">
        /// Minimum required distance (in samples) between successive peaks.  
        /// </param>
        /// <param name="prominence">
        /// Minimum required prominence (relative height compared to surrounding samples) for a peak.  
        /// </param>
        /// <returns>
        /// An array of indices corresponding to detected peaks in <paramref name="signal"/>.
        /// </returns>
        /// <remarks>
        /// If two candidate peaks occur closer than <paramref name="minDistance"/>,  
        /// the higher peak is kept and the lower one discarded.
        /// </remarks>
        /// </summary>
        public static int[] FindPeaks(float[] signal, float minHeight = 0, int minDistance = 10, float prominence = 0.01f)
        {
            List<int> peaks = new();
            for (int i = 1; i < signal.Length - 1; i++)
            {
                bool isPeak = signal[i] > signal[i - 1]
                              && signal[i] > signal[i + 1]
                              && signal[i] >= minHeight
                              && CalculateProminence(signal, at: i) > prominence;

                if (isPeak)
                {
                    if (peaks.Count == 0 || i - peaks.Last() >= minDistance)
                    {
                        peaks.Add(i);
                    }
                    else
                    {
                        // If too close, pick the higher one
                        int prev = peaks.Last();
                        if (signal[i] > signal[prev])
                        {
                            peaks[^1] = i;
                        }
                    }
                }
            }

            return peaks.ToArray();
        }
        
        private static float CalculateProminence(float[] signal, int at)
        {
            float lowestLeft = FindLowestLeft(signal, at);
            float lowestRight = FindLowestRight(signal, at);
            return signal[at] - Mathf.Max(lowestLeft, lowestRight);
        }
        
        private static float FindLowestLeft(float[] signal, int from)
        {
            float lowest = float.MaxValue;

            for (int i = from - 1; i >= 0; i--)
            {
                float leftValue = signal[i];
                if (leftValue < lowest)
                {
                    lowest = leftValue;
                }

                if (leftValue > signal[from])
                {
                    return lowest;
                }
            }

            return lowest;
        }

        private static float FindLowestRight(float[] signal, int from)
        {
            float lowest = float.MaxValue;

            for (int i = from + 1; i < signal.Length; i++)
            {
                float leftValue = signal[i];
                if (leftValue < lowest)
                {
                    lowest = leftValue;
                }

                if (leftValue > signal[from])
                {
                    return lowest;
                }
            }

            return lowest;
        }
        
        /// <summary>
        /// Clones an array but with explicit cast
        /// </summary>
        /// <param name="array"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T[] CloneArray<T>(T[] array)
        {
            return array.Clone() as T[];
        }
    }
}