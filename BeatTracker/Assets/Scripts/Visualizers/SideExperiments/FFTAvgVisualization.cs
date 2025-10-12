using System;
using System.Collections.Generic;
using AudioAnalysis;
using Graphs;
using Graphs.Processors;
using UnityEngine;

namespace Visualizers
{
    public class FFTAvgVisualization : MonoBehaviour
    {
        [SerializeField] private AudioSource audioSource;

        [Header("Graphs")] [SerializeField] private SmoothLineGraph outputDataGraph;
        [SerializeField] private SmoothLineGraph spectrumDataGraph;

        // Note: Could be converted to an array, but I reckon this is more clear for the inspector
        [SerializeField] private ScrollingLineGraph subBassGraph;
        [SerializeField] private ScrollingLineGraph bassGraph;
        [SerializeField] private ScrollingLineGraph lowMidGraph;
        [SerializeField] private ScrollingLineGraph midrangeGraph;
        [SerializeField] private ScrollingLineGraph upperMidGraph;
        [SerializeField] private ScrollingLineGraph presenceGraph;
        [SerializeField] private ScrollingLineGraph brillianceGraph;
        [SerializeField] private ScrollingLineGraph snareGraph;
        [SerializeField] private ScrollingLineGraph pianoGraph;
        [SerializeField] private ScrollingLineGraph vocalsGraph;

        private readonly float[] _samples = new float[FFTAudioAnalysisSettings.NumSamples];
        private readonly float[] _spectrumSamples = new float[FFTAudioAnalysisSettings.NumSamples];

        private Dictionary<FrequencyRange, ScrollingLineGraph> _graphBiding;

        private void Start()
        {
            _graphBiding = new Dictionary<FrequencyRange, ScrollingLineGraph>();
            _graphBiding[CommonFreqRange.SubBass] = subBassGraph;
            _graphBiding[CommonFreqRange.Bass] = bassGraph;
            _graphBiding[CommonFreqRange.LowMid] = lowMidGraph;
            _graphBiding[CommonFreqRange.Midrange] = midrangeGraph;
            _graphBiding[CommonFreqRange.UpperMid] = upperMidGraph;
            _graphBiding[CommonFreqRange.Presence] = presenceGraph;
            _graphBiding[CommonFreqRange.Brilliance] = brillianceGraph;
            _graphBiding[CommonFreqRange.Snare] = snareGraph;
            _graphBiding[CommonFreqRange.Piano] = pianoGraph;
            _graphBiding[CommonFreqRange.Vocals] = vocalsGraph;
        }

        private void Update()
        {
            // Output Data Graph
            audioSource.GetOutputData(_samples, 0);
            outputDataGraph.Process(_samples);

            // Spectrum Data
            audioSource.GetSpectrumData(_spectrumSamples, 0, FFTWindow.Hanning);
            spectrumDataGraph.Process(_spectrumSamples);

            UpdateFrequencyGraphs();
        }

        private void UpdateFrequencyGraphs()
        {
            Dictionary<FrequencyRange, float> sum = new();
            for (int i = 0; i < _samples.Length; i++)
            {
                foreach (var freq in _graphBiding.Keys)
                {
                    int startIndex = freq.StartIndex;
                    if (i >= startIndex && i <= freq.EndIndex - 1)
                    {
                        if (sum.ContainsKey(freq))
                        {
                            sum[freq] += _spectrumSamples[i];
                        }
                        else
                        {
                            sum[freq] = _spectrumSamples[i];
                        }
                    }
                }
            }

            foreach (var (range, processor) in _graphBiding)
            {
                float frequencyRange = range.EndFrequency - range.StartFrequency;
                processor.Process(sum[range] / frequencyRange);
            }
        }
    }
}