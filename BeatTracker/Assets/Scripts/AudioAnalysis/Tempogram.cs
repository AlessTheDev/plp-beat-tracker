using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using NWaves.Windows;
using UnityEngine;
using UnityEngine.Serialization;

namespace AudioAnalysis
{
    /// <summary>
    /// Computes a tempogram representation from activation values. 
    /// The tempogram estimates tempo by analyzing periodicity in onset activations 
    /// using a short-time Fourier like approach.
    /// 
    /// Original code reference:
    /// <see>
    ///     <cref>https://github.com/groupmm/real_time_plp/blob/main/realtimeplp.py</cref>
    /// </see>
    /// </summary>
    ///
    public class Tempogram
    {
        private readonly float[] _tempoBuffer;
        private Complex[] _expCache; // Cache of complex exponential coefficients

        private readonly float[] _theta;
        private readonly float[] _hanningWindow;

        private readonly float _frameRate;
        private readonly int _windowSize;

        private readonly int _hopSizeFrames = 1; // Compute every frame
        
        /// <summary>
        /// Initializes a new instance of the <see cref="Tempogram"/> class.
        /// </summary>
        /// <param name="theta">
        /// An array of ordered tempo candidate frequencies (in BPM) -> Example [60;180].
        /// These values determine the resolution of tempo estimation.
        /// </param>
        /// <param name="hanningWindow">
        /// The window function applied to the buffer (usually a Hann window). 
        /// </param>
        /// <param name="frameRate">
        /// The rate (in frames per second) at which activation values are sampled. 
        /// Used to map buffer indices to time.
        ///
        /// Usually calculated as: sampleRate / hopSize
        /// </param>
        public Tempogram(float[] theta, float[] hanningWindow, float frameRate)
        {
            _frameRate = frameRate;
            _windowSize = hanningWindow.Length;

            _hanningWindow = hanningWindow;
            _theta = theta;
            
            // Tempo buffer is full length _windowSize, but only the left half will be updated
            _tempoBuffer = new float[_windowSize];

            PrecomputeExponential();
        }
        
        /// <summary>
        /// Precomputes the complex exponential coefficients for each tempo candidate.
        /// These are reused to avoid redundant calculations in <see cref="Process"/>.
        /// </summary>
        private void PrecomputeExponential()
        {
            int l = _windowSize;
            int k = _theta.Length;

            _expCache = new Complex[l * k];

            for (int i = 0; i < k; i++) 
            {
                float omega = _theta[i] / 60f;
                for (int j = 0; j < l; j++)
                {
                    float m = j / _frameRate;
                    float angle = -2 * Mathf.PI * omega * m;
                    _expCache[i * l + j] = Complex.Exp(new Complex(0, angle));
                }
            }
        }

        /// <summary>
        /// Processes a new activation frame and updates the tempogram.
        /// </summary>
        /// <param name="activationFrame">
        /// A frame of novelty or activation values to add to the buffer.
        /// </param>
        /// <returns>
        /// A complex vector representing the tempogram across tempo candidates. 
        /// The magnitude of each element indicates the strength of that tempo, 
        /// and the phase encodes alignment information.
        /// </returns>
        public Complex[] Process(float[] activationFrame)
        {
            HalfWindowMethod(activationFrame);
            
            int l = _windowSize;
            int k = _theta.Length;
            var x = new Complex[k];

            for (int i = 0; i < k; i++) // i is the "tempo index"
            {
                Complex sum = Complex.Zero;

                // Calculate the sum
                for (int j = 0; j < l; j++)
                {
                    sum += _tempoBuffer[j] * _hanningWindow[j] * _expCache[i * l + j];
                }

                x[i] = sum;
            }

            return x;
        }

        /// <summary>
        /// Updates the tempo buffer using the half-window method:
        /// - Rolls the left half of the buffer by one frame.
        /// - Inserts new activation values at the end of the rolled half.
        /// - Keeps the right half zero because that's the future.
        /// </summary>
        /// <param name="activationFrame">The new activation values to insert.</param>
        private void HalfWindowMethod(float[] activationFrame)
        {
            if (activationFrame.Length == 0) return;
            
            int halfLength = _tempoBuffer.Length / 2;
            
            // Extract left half
            float[] novHalf = new float[halfLength];
            Array.Copy(_tempoBuffer, 0, novHalf, 0, halfLength);
            
            // Roll left half by hopSizeFrames (should be 1)
            float[] rolledHalf = new float[halfLength];
            for (int i = 0; i < halfLength; i++)
            {
                int sourceIndex = (i + _hopSizeFrames) % halfLength;
                rolledHalf[i] = novHalf[sourceIndex];
            }
            
            // Insert new activation values at the end
            int insertCount = Mathf.Min(activationFrame.Length, _hopSizeFrames);
            for (int i = 0; i < insertCount; i++)
            {
                rolledHalf[halfLength - _hopSizeFrames + i] = activationFrame[i];
            }
            
            // Copy updated half back to main buffer
            Array.Copy(rolledHalf, 0, _tempoBuffer, 0, halfLength);
            
            // Right half remains zero
            for (int i = halfLength; i < _tempoBuffer.Length; i++)
            {
                _tempoBuffer[i] = 0f;
            }
        }
    }
}