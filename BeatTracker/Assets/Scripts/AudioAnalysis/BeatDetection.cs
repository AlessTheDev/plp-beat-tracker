using System;
using System.Linq;
using UnityEngine;

namespace AudioAnalysis
{
    /// <summary>
    /// Provides methods for detecting beat positions from the Predominant Local Pulse (PLP) curve.  
    /// 
    /// Original code reference:
    /// <see>
    ///     <cref>https://github.com/groupmm/real_time_plp/blob/main/realtimeplp.py</cref>
    /// </see>
    /// </summary>
    public class BeatDetection
    {
        private int _lastBeatDistance;
        private float _maxPeakAmplitude;
        
        /// <summary>
        /// Gets the stability of the last detected beat, normalized to [0, 1].  
        /// Higher values indicate more reliable beat detections.
        /// </summary>
        public float Stability {get; private set;}

        private readonly int _windowSize;
        private readonly float[] _hanningWindow;
        private readonly int _cursor;
        
        // The C constant from the paper
        private readonly float _normalizationConstant;
        
        private readonly float[] _alpha;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="BeatDetection"/> class.
        /// </summary>
        /// <param name="windowSize">
        /// The size of the analysis window used for the PLP buffer.
        /// </param>
        /// <param name="hanningWindow">
        /// The window function used in overlap-add calculations for alpha normalization.
        /// Typically, a Hann window matching <paramref name="windowSize"/>
        /// </param>
        public BeatDetection(int windowSize, float[] hanningWindow)
        {
            _lastBeatDistance = int.MaxValue;
            _maxPeakAmplitude = -1;
            
            _windowSize = windowSize;
            _hanningWindow = hanningWindow;
            
            _cursor = windowSize / 2;
            
            _normalizationConstant = hanningWindow.Sum();
            
            _alpha = CalculateAlpha();
        }
        
        /// <summary>
        /// Attempts to detect a beat at the current cursor position in the PLP buffer.  
        /// Uses either raw peak detection or alpha-normalized PLP depending on <paramref name="useAlpha"/>.
        /// </summary>
        /// <param name="pulseBuffer">The PLP buffer values to analyze.</param>
        /// <param name="useAlpha">
        /// If true, uses the alpha-normalized PLP to detect peaks.  
        /// If false, uses raw peak detection.
        /// </param>
        /// <returns>
        /// True if a new beat was detected at the cursor position, otherwise false.
        /// </returns>
        public bool Detect(float[] pulseBuffer, bool useAlpha = false)
        {
            int[] peaksIndexes = useAlpha ? GetPeakTimePositions(pulseBuffer) : AudioUtils.FindPeaks(pulseBuffer);

            // Get Closest Future beat
            var futureBeats = peaksIndexes.Where(i => i > _cursor).ToArray();
            int closestFutureBeat = futureBeats.Length > 0 ? futureBeats[0] : 0;
            int distanceToClosestFutureBeat = closestFutureBeat - _cursor;

            bool beatDetected = distanceToClosestFutureBeat > _lastBeatDistance;
            _lastBeatDistance = distanceToClosestFutureBeat;

            if (!beatDetected) return false;
            
            float peakAmplitude = pulseBuffer[_cursor];
            if (peakAmplitude > _maxPeakAmplitude)
            {
                _maxPeakAmplitude = peakAmplitude;
            }

            Stability = Mathf.Clamp01(peakAmplitude / _maxPeakAmplitude);

            return true;
        }

        /// <summary>
        /// Finds peak time positions using the alpha-normalized PLP curve.
        /// </summary>
        /// <param name="pulseBuffer">The PLP buffer to analyze.</param>
        /// <returns>An array of indices corresponding to detected peak positions.</returns>
		public int[] GetPeakTimePositions(float[] pulseBuffer)
        {
            float[] alphaPlp = CalculateAlphaPLP(pulseBuffer);
            int[] peaksIndexes = AudioUtils.FindPeaks(alphaPlp);
            return peaksIndexes;
        }

        #region AlphaPLP Related Computations

        private float[] CalculateAlpha()
        {
            int h = 1;
            int l = _windowSize * 2;

            float[] kernelOverlap = OverlapAddKernelWindows(_hanningWindow, h, l);

            int startIndex = kernelOverlap.Length / 2;
            int alphaLen = kernelOverlap.Length - startIndex;
            
            var alpha = new float[alphaLen];
            
            for (int i = 0; i < alphaLen; i++)
            {
                alpha[i] = kernelOverlap[i + startIndex] / _normalizationConstant;
                if (alpha[i] == 0)
                {
                    alpha[i] = Mathf.Epsilon; // Avoid division by 0
                }
            }
            
            return alpha;
        }
        
        /// <summary>
        /// Computes the overlap-add of multiple shifted window functions.  
        /// Used for building the normalization kernel in alpha computation.
        /// </summary>
        /// <param name="window">The Hann window to overlap and add.</param>
        /// <param name="h">The hop size (in frames) between overlapping windows.</param>
        /// <param name="l">The total length of the overlap-add buffer.</param>
        private float[] OverlapAddKernelWindows(float[] window, int h, int l)
        {
            int m = (l - _windowSize) / h + 1;
            float[] winSum = new float[l];
            for (int i = 0; i < m; i++)
            {
                int start = i * h;
                for (int j = 0; j < _windowSize; j++)
                {
                    if (start + j < l)
                    {
                        winSum[start + j] += window[j];
                    } 
                }
            }
            return winSum;
        }
        
        // ReSharper disable twice InconsistentNaming
        /// <summary>
        /// Computes the alpha-normalized PLP curve from the raw PLP buffer.  
        /// This compensates for temporal windowing effects.
        /// </summary>
        /// <param name="pulseBuffer">The raw PLP buffer values.</param>
        /// <returns>The alpha-normalized PLP values.</returns>
        public float[] CalculateAlphaPLP(float[] pulseBuffer)
        {
            float[] alphaPLP = new float[_alpha.Length];
            for (int i = 0; i < alphaPLP.Length; i++)
            {
                float norm = pulseBuffer[i] / _normalizationConstant;
                alphaPLP[i] = norm / _alpha[i];
            }
            return alphaPLP;
        }
        #endregion
    }
}