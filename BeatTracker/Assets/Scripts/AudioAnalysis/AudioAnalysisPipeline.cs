using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using NWaves.Windows;
using UnityEngine;

namespace AudioAnalysis
{
    /// <summary>
    /// This class connects all the processes involving audio analysis.
    /// 1. Get audio Data through the `AudioDataProcessor`
    /// 2. Input that data to the `BeatActivation` function
    /// 3. Proceed by creating a `Tempogram`
    /// 4. Finally get the `PredominantLocalPulse` and identify peaks
    /// --
    /// Note: most of this code could be simplified if we avoid to cache variables.
    /// We are doing that only for visualization purposes
    /// since other classes will have to access those properties in order to create graphs.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class AudioAnalysisPipeline : MonoBehaviour
    {
        [Header("Audio Settings")] 
		[SerializeField] private int numSamples = 1024;
        [SerializeField] private int hopSize = 512;

        [Space(10)] [SerializeField] private int activationWindowSize = 1024;

        // Beat Activation Settings
        [SerializeField] private int halfCentricLocAvgWindowSize = 10;
        [SerializeField] private float gamma = 1000; // Parameter for logarithmic compression

        // Tempogram and PLP Settings
        [SerializeField] private float timeWindowSize = 9;
        [SerializeField] private bool useAlpha = false;

        [Space(10)] 
        [SerializeField] private int bpmBufferSize = 200;

        // Elements of the pipeline
        private AudioDataProcessor _audioDataProcessor;
        private BeatActivation _beatActivation;
        private Tempogram _tempogram;
        private PredominantLocalPulse _plp;
        private BeatDetection _beatDetection;

        private CirclularBuffer<float> _bpmBuffer;

        #region Public Accessors
        public float[] OutputData { get; private set; }
        public float[] ActivationData { get; private set; }
        public Complex[] TempogramData { get; private set; }
        public float[] PlpData { get; private set; }
        public int[] BeatTimePositions { get; private set; }

        public int Bpm => Mathf.RoundToInt(_bpmBuffer.Values.Average());

        public float[] HanningWindow { get; private set; }
        public float[] Theta { get; private set; }

        public float Framerate { get; private set; }

        public bool BeatDetected { get; private set; }
        public float BeatStability { get; private set; }

        #endregion

        private void Awake()
        {
            Framerate = AudioSettings.outputSampleRate / (float)hopSize;
            int windowSize = Mathf.RoundToInt(timeWindowSize * Framerate);

            _bpmBuffer = new CirclularBuffer<float>(bpmBufferSize);

            HanningWindow = Window.Hann(windowSize);
            Theta = GetTheta(60, 180);

			// Initialize Processors
            _audioDataProcessor = new AudioDataProcessor(numSamples, hopSize, ProcessSamples);
            _beatActivation = new BeatActivation(activationWindowSize, halfCentricLocAvgWindowSize, gamma);
            _tempogram = new Tempogram(Theta, HanningWindow, Framerate);
            _plp = new PredominantLocalPulse(Theta, HanningWindow, windowSize, Framerate);
            _beatDetection = new BeatDetection(windowSize, HanningWindow);
        }

        private void OnAudioFilterRead(float[] data, int channels)
        {
            _audioDataProcessor.Process(data, channels);
        }

        /// <summary>
        /// Called every time a new samples window is ready
        /// </summary>
        /// <param name="samples">The sampled passed in by the AudioDataProcessor</param>
        private void ProcessSamples(float[] samples)
        {
            // Process data in the pipeline
            OutputData = samples;
            ActivationData = _beatActivation.Process(samples);
            TempogramData = _tempogram.Process(ActivationData);
            PlpData = _plp.Process(TempogramData);
            BeatTimePositions = _beatDetection.GetPeakTimePositions(PlpData);
            
            bool beatDetected = _beatDetection.Detect(PlpData, useAlpha);

            // Avoid skipping beats
            if (beatDetected)
            {
                BeatDetected = true; // Will be set to false in late update

                BeatStability = Mathf.Max(_beatDetection.Stability, BeatStability);
            }

            _bpmBuffer.Add(_plp.Tempo);
        }

        private void LateUpdate()
        {
            BeatDetected = false;
            BeatStability = 0;
        }

        /// <summary>
        /// Returns an array containing integer values from start to end (inclusive)
        /// </summary>
        /// <param name="start">The first integer</param>
        /// <param name="end">The last integer</param>
        /// <returns>an array containing integer values from start to end (inclusive)</returns>
        public static float[] GetTheta(int start, int end)
        {
            int range = end - start + 1;
            float[] theta = new float[range];

            for (int i = 0; i < range; i++)
            {
                theta[i] = start + i;
            }

            return theta;
        }
    }
}