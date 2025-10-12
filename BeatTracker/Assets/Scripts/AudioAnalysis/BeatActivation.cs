using System.Linq;
using NWaves.Transforms;
using NWaves.Windows;
using UnityEngine;

namespace AudioAnalysis
{
    /// <summary>
    /// The activation function answers the question: How likely there's a beat right now?
    ///
    /// The peaks of the activation function indicate the likelihood of observing a beat at time positions
    /// 
    /// Original code reference:
    /// <see>
    ///     <cref>https://github.com/groupmm/real_time_plp/blob/main/realtimeplp.py</cref>
    /// </see>
    /// </summary>
    public class BeatActivation
    {
        private readonly float _gamma; // Log compression parameter

        private readonly CirclularBuffer<float> _windowBuffer;
        private readonly CirclularBuffer<float> _laBuffer;
        
        private float[] _previousFrame; // Used to calculate the Discrete Derivative
        
        private readonly Stft _stft;

        private readonly float _laMul; // it's faster to compute this once and then use it later -> (1f / halfCentricLocAvgWindowSize) 

        private float[][] _processedSpectrogram;

        private readonly bool _useLogCompression;

        /// <summary>
        /// Initializes a new instance of the <see cref="BeatActivation"/> class.
        /// </summary>
        /// <param name="windowSize">
        /// The size of the analysis window (in samples) used for the Short-Time Fourier Transform (STFT).
        /// </param>
        /// <param name="halfCentricLocAvgWindowSize">
        /// The half-length of the window used to compute the local average for normalization. 
        /// Determines the smoothing of novelty values.
        /// </param>
        /// <param name="gamma">
        /// The log compression parameter applied to the spectrogram magnitudes to reduce dynamic range.
        /// </param>
        /// <param name="useLogCompression">If true it will process the spectrogram using logarithmic compression</param>
        public BeatActivation(int windowSize,  int halfCentricLocAvgWindowSize, float gamma, bool useLogCompression = true)
        {
            int hopSize = windowSize / 2;
            _gamma = gamma;
            _useLogCompression = useLogCompression;
            
            _windowBuffer = new CirclularBuffer<float>(windowSize);
            _laBuffer = new CirclularBuffer<float>(halfCentricLocAvgWindowSize);

            _stft = new Stft(windowSize, hopSize, WindowType.Hann);
            _laMul = 1f / halfCentricLocAvgWindowSize;
        }
        
        /// <summary>
        /// Processes a new frame of audio samples and computes the beat activation values.
        /// </summary>
        /// <param name="frame">
        /// The input audio frame to analyze. The frame length should match the configured window size.
        /// </param>
        /// <returns>
        /// An array of novelty values representing the likelihood of beat occurrences.e.
        /// </returns>
        public float[] Process(float[] frame)
        {
            _windowBuffer.Add(frame);

            return ProcessFromBuffer();
        }

        private float[] ProcessFromBuffer()
        {
            // Base STFT
            var spectrogram = _stft.MagnitudePhaseSpectrogram(_windowBuffer.ToOrderedArray()).Magnitudes; 

            // Convert to 2D float array
            int numFrames = spectrogram.Count; // = 2
            int numBins = spectrogram[0].Length;

            // Post-processing (online spectrogram)
            _processedSpectrogram = new float[numFrames][];
            for (int f = 0; f < numFrames; f++)
            {
                float[] newFrame = new float[numBins];
                for (int b = 0; b < numBins; b++)
                {
                    newFrame[b] = _useLogCompression ? Mathf.Log(1 + _gamma * spectrogram[f][b]) : spectrogram[f][b]; // No need to replicate np.abs since we apply the calculations to the magnitude directly
                }
                _processedSpectrogram[f] = newFrame;
            }
            
            // Handle first frame
            if (_previousFrame == null)
            {
                UpdatePreviousFrame(_processedSpectrogram);
                return new []{ 0f };
            }
            
            // Discrete Derivative
            float[][] spectrogramDiff = new float[numFrames][];
            for (int f = 0; f < numFrames; f++)
            {
                float[] frameDiff = new float[numBins];
                for (int b = 0; b < numBins; b++)
                {
                    float prev = f == 0 
                        ? _previousFrame[b]   // For first new frame, prepend with last frame from previous call
                        : _processedSpectrogram[f - 1][b];

                    float diff = _processedSpectrogram[f][b] - prev;
                    
                    // Half-Wave rectification
                    if (diff < 0f)
                    {
                        diff = 0;
                    }
                    
                    frameDiff[b] = diff; 
                }
                spectrogramDiff[f] = frameDiff;
            }

            UpdatePreviousFrame(_processedSpectrogram);
            
            // Accumulate novelty across frequency bins
            // In other words, it collapses each frame into a single novelty value
            // (amount of spectral change at that time step).
            float[] nov = new float[numFrames];
            for (int f = 0; f < numFrames; f++)
            {
                nov[f] = spectrogramDiff[f].Sum();
            }
            
            // Local Average
            _laBuffer.Add(nov[0]);
            
            float localAverage = 0;
            for (int i = 0; i < _laBuffer.Length; i++)
            {
                localAverage += _laBuffer.GetRaw(i);
            }
            localAverage *= _laMul;
            
            // Normalization
            float[] novNorm =  new float[nov.Length];
            for (int i = 0; i < novNorm.Length; i++)
            {
                float n = nov[i] - localAverage;

                if (n < 0) n = 0;
                
                novNorm[i] = n;
            }
            
            return new[] { novNorm.Max() };
        }

        private void UpdatePreviousFrame(float[][] processedSpectrogram)
        {
            _previousFrame = processedSpectrogram[^1];
        }
        
        public float[] ProcessedSpectrogram => _processedSpectrogram[0];
    }
}