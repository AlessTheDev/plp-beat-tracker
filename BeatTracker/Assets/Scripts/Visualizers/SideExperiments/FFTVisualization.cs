using System;
using System.Collections.Generic;
using AudioAnalysis;
using Graphs;
using Graphs.Processors;
using UnityEngine;

namespace Visualizers
{
    public class FFTVisualization : MonoBehaviour
    {
        [SerializeField] private AudioSource audioSource;
        
        [Header("Graphs")]
        [SerializeField] private SmoothLineGraph outputDataGraph;
        [SerializeField] private SmoothLineGraph spectrumDataGraph;
        
        // Note: Could be converted to an array, but I reckon this is more clear for the inspector
        [SerializeField] private SmoothLineGraph subBassGraph;
        [SerializeField] private SmoothLineGraph bassGraph;
        [SerializeField] private SmoothLineGraph lowMidGraph;
        [SerializeField] private SmoothLineGraph midrangeGraph;
        [SerializeField] private SmoothLineGraph upperMidGraph;
        [SerializeField] private SmoothLineGraph presenceGraph;
        [SerializeField] private SmoothLineGraph brillianceGraph;
        [SerializeField] private SmoothLineGraph snareGraph;
        [SerializeField] private SmoothLineGraph pianoGraph;
        [SerializeField] private SmoothLineGraph vocalsGraph;
        

        private readonly float[] _samples = new float[FFTAudioAnalysisSettings.NumSamples];
        private readonly float[] _spectrumSamples = new float[FFTAudioAnalysisSettings.NumSamples];

        private Dictionary<FrequencyRange, float[]> _samplesSections;

        private void Start()
        {
            _samplesSections = new Dictionary<FrequencyRange, float[]>();
            
            SetupFrequencyRangeSections();
            SetupGraphsHorizontalAxis();
        }

        private void SetupGraphsHorizontalAxis()
        {
            AssignRangeToAxis(CommonFreqRange.SubBass, subBassGraph);
            AssignRangeToAxis(CommonFreqRange.Bass, bassGraph);
            AssignRangeToAxis(CommonFreqRange.LowMid, lowMidGraph);
            AssignRangeToAxis(CommonFreqRange.Midrange, midrangeGraph);
            AssignRangeToAxis(CommonFreqRange.UpperMid, upperMidGraph);
            AssignRangeToAxis(CommonFreqRange.Presence, presenceGraph);
            AssignRangeToAxis(CommonFreqRange.Brilliance, brillianceGraph);
            AssignRangeToAxis(CommonFreqRange.Snare, snareGraph);
            AssignRangeToAxis(CommonFreqRange.Piano, pianoGraph);
            AssignRangeToAxis(CommonFreqRange.Vocals, vocalsGraph);
        }

        private void AssignRangeToAxis(FrequencyRange range, LineGraph graphProcessor)
        {
            graphProcessor.SetRangeOverride(range.StartFrequency, range.EndFrequency);
        }

        private void SetupFrequencyRangeSections()
        {
            AddFreqRangeSectionArray(CommonFreqRange.SubBass);
            AddFreqRangeSectionArray(CommonFreqRange.Bass);
            AddFreqRangeSectionArray(CommonFreqRange.LowMid);
            AddFreqRangeSectionArray(CommonFreqRange.Midrange);
            AddFreqRangeSectionArray(CommonFreqRange.UpperMid);
            AddFreqRangeSectionArray(CommonFreqRange.Brilliance);
            AddFreqRangeSectionArray(CommonFreqRange.Presence);
            AddFreqRangeSectionArray(CommonFreqRange.Snare);
            AddFreqRangeSectionArray(CommonFreqRange.Piano);
            AddFreqRangeSectionArray(CommonFreqRange.Vocals);
        }

        private void AddFreqRangeSectionArray(FrequencyRange range)
        {
            _samplesSections[range] = new float[range.EndIndex - range.StartIndex];
        }

        private void Update()
        {
            // Output Data Graph
            audioSource.GetOutputData(_samples, 0);
            outputDataGraph.Process(_samples);
            
            // Spectrum Data
            audioSource.GetSpectrumData(_spectrumSamples, 0, FFTWindow.Hanning);
            spectrumDataGraph.Process(_spectrumSamples);
            UpdateFrequencySections();
            UpdateFrequencySectionGraphs();
        }

        private void UpdateFrequencySectionGraphs()
        {
            subBassGraph.Process(_samplesSections[CommonFreqRange.SubBass]);
            bassGraph.Process(_samplesSections[CommonFreqRange.Bass]);
            lowMidGraph.Process(_samplesSections[CommonFreqRange.LowMid]);
            midrangeGraph.Process(_samplesSections[CommonFreqRange.Midrange]);
            upperMidGraph.Process(_samplesSections[CommonFreqRange.UpperMid]);
            brillianceGraph.Process(_samplesSections[CommonFreqRange.Brilliance]);
            presenceGraph.Process(_samplesSections[CommonFreqRange.Presence]);
            snareGraph.Process(_samplesSections[CommonFreqRange.Snare]);
            pianoGraph.Process(_samplesSections[CommonFreqRange.Piano]);
            vocalsGraph.Process(_samplesSections[CommonFreqRange.Vocals]);
        }

        private void UpdateFrequencySections()
        {
            for (int i = 0; i < _samples.Length; i++)
            {
                foreach (var section in _samplesSections)
                {
                    int startIndex = section.Key.StartIndex;
                    if (i >= startIndex && i <= section.Key.EndIndex - 1)
                    {
                        section.Value[i - startIndex] = _spectrumSamples[i];
                    }
                }
            }
        }
    }
}
