using System;
using AudioAnalysis;
using Graphs;
using Graphs.Processors;
using NWaves.Transforms;
using NWaves.Windows;
using UnityEngine;

namespace Visualizers
{
    [RequireComponent(typeof(AudioSource))]
    public class STFTVisualization : MonoBehaviour
    {
        [SerializeField] private SmoothLineGraph outputDataGraph;
        [SerializeField] private SmoothLineGraph stftFrame1Graph;
        [SerializeField] private SmoothLineGraph stftFrame2Graph;

        private Stft _stft;
        private AudioDataProcessor _audioDataProcessor;

        private float[] _sampleValues;
        private float[] _firstFrame;
        private float[] _secondFrame;
        
        private void Awake()
        {
            _audioDataProcessor = new AudioDataProcessor(1024, 512, OnSamplesReady);
            _stft = new Stft(1024, 512, WindowType.Hann);
        }

        private void Update()
        {
            outputDataGraph.Process(_sampleValues);
            stftFrame1Graph.Process(_firstFrame);
            stftFrame2Graph.Process(_secondFrame);
        }

        private void OnAudioFilterRead(float[] data, int channels)
        {
            _audioDataProcessor.Process(data, channels);
        }

        private void OnSamplesReady(float[] samples)
        {
            var spectrogram = _stft.MagnitudePhaseSpectrogram(samples).Magnitudes;
            _sampleValues = AudioUtils.CloneArray(samples);
            _firstFrame = AudioUtils.CloneArray(spectrogram[0]);
            _secondFrame = AudioUtils.CloneArray(spectrogram[1]);

        }
    }
}
