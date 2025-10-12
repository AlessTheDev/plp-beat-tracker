using System;
using System.Linq;
using System.Numerics;
using AudioAnalysis;
using Graphs.Processors;
using NWaves.Transforms;
using NWaves.Windows;
using UnityEngine;

namespace Visualizers
{
    [RequireComponent(typeof(AudioSource))]
    public class Playground : MonoBehaviour
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
        
        [Header("Graphs")]
        [SerializeField] private SmoothLineGraph rawSpectrogramGraph;
        [SerializeField] private SmoothLineGraph processedSpectrogramGraph;
        
        // Elements of the pipeline
        private AudioDataProcessor _audioDataProcessor;
        private BeatActivation _beatActivation;
        private BeatActivation _beatActivationNoComp;
        private Tempogram _tempogram;
        private PredominantLocalPulse _plp;
        private BeatDetection _beatDetection;
        
        private float[] _hanningWindow;
        private float[] _theta;

        private float[] _outputData;
        private float[] _rawSpectrogram;
        private float[] _processedSpectrogram;
        private float[] _activationData;
        private Complex[] _tempogramData;
        private float[] _plpData;
        private float[] _alphaPlpData;
        private int[] _beatPositions;
        
        private Stft _stft;
        
        private void Awake()
        {
            _stft = new Stft(numSamples, hopSize);
                
            float framerate = AudioSettings.outputSampleRate / (float)hopSize;
            int windowSize = Mathf.RoundToInt(timeWindowSize * framerate);

            _hanningWindow = Window.Hann(windowSize);
            _theta = AudioAnalysisPipeline.GetTheta(60, 180);

            // Initialize Processors
            _audioDataProcessor = new AudioDataProcessor(numSamples, hopSize, ProcessSamples);
            _beatActivation = new BeatActivation(activationWindowSize, halfCentricLocAvgWindowSize, gamma);
            _beatActivationNoComp = new BeatActivation(activationWindowSize, halfCentricLocAvgWindowSize, gamma, false);
            _tempogram = new Tempogram(_theta, _hanningWindow, framerate);
            _plp = new PredominantLocalPulse(_theta, _hanningWindow, windowSize, framerate);
            _beatDetection = new BeatDetection(windowSize, _hanningWindow);
        }

        private void OnAudioFilterRead(float[] data, int channels)
        {
            _audioDataProcessor?.Process(data, channels);
        }

        private void ProcessSamples(float[] samples)
        {
            _outputData = samples;
            
            _beatActivationNoComp.Process(samples);
            _rawSpectrogram = _beatActivationNoComp.ProcessedSpectrogram;
            
            _activationData = _beatActivation.Process(samples);
            _processedSpectrogram = _beatActivation.ProcessedSpectrogram;
            
            _tempogramData = _tempogram.Process(_activationData);
            _plpData = _plp.Process(_tempogramData);
            _beatPositions = _beatDetection.GetPeakTimePositions(_plpData);
            _alphaPlpData = _beatDetection.CalculateAlphaPLP(_plpData);
        }

        private void Update()
        {
            if (_processedSpectrogram == null) return;
            
            rawSpectrogramGraph.Process(_rawSpectrogram);
            processedSpectrogramGraph.Process(_processedSpectrogram);
        }
        

    }
}
