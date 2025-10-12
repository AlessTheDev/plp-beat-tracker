using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using NWaves.Windows;
using UnityEngine;
using UnityEngine.Internal;
using UnityEngine.Serialization;

namespace AudioAnalysis
{
    /// <summary>
    /// Computes the Predominant Local Pulse (PLP) curve from a tempogram.  
    /// The PLP estimates the pulse of a track based on the most dominant tempo and phase.
    ///  
    /// Unlike the original paper, the normalization constant C is only used 
    /// for the alpha calculation in <see cref="BeatDetection"/>. 
    /// 
    /// Original code reference:
    /// <see>
    ///     <cref>https://github.com/groupmm/real_time_plp/blob/main/realtimeplp.py</cref>
    /// </see>
    /// </summary>
    public class PredominantLocalPulse
    {
        private readonly CirclularBuffer<float> _pulseBuffer;
        
        // Allocate only once
        private readonly float[] _waveform; 
        
        private readonly int _windowSize;
        private readonly float _frameRate;
        
        private readonly float[] _theta;
        private readonly float[] _hanningWindow;
        

        public float Tempo { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PredominantLocalPulse"/> class.
        /// </summary>
        /// <param name="theta">
        /// An array of tempo candidate values (BPM).
        /// </param>
        /// <param name="hanningWindow">
        /// The analysis window applied when generating the kernel waveform.  
        /// Typically, a Hann window matching <paramref name="windowSize"/>.
        /// </param>
        /// <param name="windowSize">
        /// The size of the analysis window (in frames). Determines the length of the PLP buffer.
        /// </param>
        /// <param name="frameRate">
        /// The rate (in frames per second) at which activation or tempogram frames are sampled.  
        /// Used to convert tempo (BPM) into frequency in Hz.
        /// </param>
        public PredominantLocalPulse(float[] theta, float[] hanningWindow, int windowSize, float frameRate)
        {
            _windowSize = windowSize;
            _frameRate = frameRate;

            _hanningWindow = hanningWindow;
            _theta = theta;
            
            _pulseBuffer = new CirclularBuffer<float>(_windowSize);
            _waveform = new float[_windowSize];

        }
        
        /// <summary>
        /// Processes a new tempogram frame and updates the PLP curve.
        /// </summary>
        /// <param name="tempogramFrame">
        /// The complex-valued tempogram frame (tempo candidates x strength).  
        /// </param>
        /// <returns>
        /// The updated PLP curve as a float array.  
        /// Peaks in the curve correspond to estimated beat locations.
        /// </returns>
        public float[] Process(Complex[] tempogramFrame)
        {
            // Roll by 1
            _pulseBuffer.Add(0);
            
            FillKernelWaveform(tempogramFrame, out int tempo);
            Tempo = tempo;

            // Overlap add
            for (int i = 0; i < _waveform.Length; i++)
            {
                _pulseBuffer.Values[_pulseBuffer.GetIndexFIFO(i)] += _waveform[i];
            }
            
            return _pulseBuffer.ToOrderedArray();
        }
        
        /// <summary>
        /// Builds the kernel waveform corresponding to the dominant tempo in the tempogram frame.  
        /// The kernel is a Hann-windowed cosine wave aligned to the phase of the chosen tempo component.
        /// </summary>
        /// <param name="tempogramFrame">The current tempogram frame.</param>
        /// <param name="tempo">The detected dominant tempo (in BPM).</param>
        private void FillKernelWaveform(Complex[] tempogramFrame, out int tempo)
        {
            int dominantTempoIndex = GetDominantTempoIndex(tempogramFrame);
            tempo = Mathf.RoundToInt(_theta[dominantTempoIndex]);
            float omega = tempo / 60f / _frameRate;
            float phase = (float)(-tempogramFrame[dominantTempoIndex].Phase / (2 * Mathf.PI));

            for (int i = 0; i < _windowSize; i++)
            {
                _waveform[i] = _hanningWindow[i] * Mathf.Cos(2 * Mathf.PI * (i * omega - phase));
            }
        }
        
        /// <summary>
        /// Finds the index of the dominant tempo candidate in a tempogram frame 
        /// by selecting the candidate with the maximum magnitude.
        /// Similar to Argmax, for those who are familiar with python.
        /// </summary>
        /// <param name="tempogramFrame">The tempogram frame to analyze.</param>
        /// <returns>The index of the dominant tempo candidate.</returns>
        private static int GetDominantTempoIndex(Complex[] tempogramFrame)
        {
            int dominantTempoIndex = 0;
            float highestMag = -1;

            for (int i = 0; i < tempogramFrame.Length; i++)
            {
                float currMag = (float)tempogramFrame[i].Magnitude;
                if (currMag > highestMag)
                {
                    highestMag = currMag;
                    dominantTempoIndex = i;
                }
            }

            return dominantTempoIndex;
        }
    }
}