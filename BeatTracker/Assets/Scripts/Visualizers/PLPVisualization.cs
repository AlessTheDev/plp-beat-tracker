using System;
using System.Collections;
using System.Numerics;
using AudioAnalysis;
using Graphs;
using Graphs.Processors;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Vector2 = UnityEngine.Vector2;

namespace Visualizers
{
    // ReSharper disable once InconsistentNaming
    /// <summary>
    /// Visualization code for the main PLP algorithm including:
    /// - Audio Output Data
    /// - Activation Function
    /// - Tempogram
    /// - PLP
    /// And also
    /// - A circle that pulses on beat
    /// - BPMs
    /// - Stability
    /// </summary>
    public class PLPVisualization : MonoBehaviour
    {
        private static readonly int Clap = Animator.StringToHash("Clap");
        [SerializeField] private AudioAnalysisPipeline audioAnalysis;
        [SerializeField] private float updateFrequency = -1;

        [SerializeField] private TextMeshProUGUI bpmText;
        [SerializeField] private TextMeshProUGUI stabilityText;
        
        [Header("Pulse Images")]
        [SerializeField] private PulseImage rawBeatPulse;
        [SerializeField] private PulseImage stableBeatPulse;
        [SerializeField] private float stabilityThreshold;
        
        [Header("Graphs")] 
        [SerializeField] private SmoothLineGraph outputDataGraph;
        [SerializeField] private ScrollingLineGraph activationGraph;
        [SerializeField] private TempogramGraph tempogramGraph;
        [SerializeField] private SmoothLineGraph plpGraph;
        [SerializeField] private BinaryGraph beatsGraph;

        [Header("Other")] [SerializeField] private Animator clapCatAnimator;

        private float _lastUpdate;

        private void Start()
        {
            // Try to run at max performance if possible
            Application.targetFrameRate = -1;
            QualitySettings.vSyncCount = 0;
        }

        private void Update()
        {
            if (audioAnalysis.OutputData == null || updateFrequency > 0 && Time.time - _lastUpdate < UpdateInterval)
            {
                return;
            }
            
            _lastUpdate = Time.time;
            
            // Output graph update
            outputDataGraph.Process(audioAnalysis.OutputData);
            
            // Activation graph update
            activationGraph.Process(audioAnalysis.ActivationData);

            // Tempogram Update
            tempogramGraph.InitializeIfNeeded(audioAnalysis.TempogramData.Length, audioAnalysis.Theta);
            tempogramGraph.Process(audioAnalysis.TempogramData);
            
            // PLP Graph Update
            plpGraph.Process(audioAnalysis.PlpData);
            
            float halfLength = audioAnalysis.PlpData.Length / 2f / audioAnalysis.Framerate;
            plpGraph.SetRangeOverride(-halfLength, halfLength); // Make sure there's a 0 in the graph's center

            // Peaks Graph Update
            int[] peakPos = audioAnalysis.BeatTimePositions;

            beatsGraph.Graph.XRange.SetRange(0, audioAnalysis.PlpData.Length);
            beatsGraph.Process(peakPos);
            
			beatsGraph.Graph.XRange.SetRange(-halfLength, halfLength);

            // Update text information
            bpmText.text = $"BPM: {audioAnalysis.Bpm / 2} or {audioAnalysis.Bpm}";

            if (audioAnalysis.BeatDetected)
            {
                string stabilityInfo = audioAnalysis.BeatStability < 0.45f ? "(Unstable)" : "(Stable)";
                stabilityText.text = $"Stability: {audioAnalysis.BeatStability:0.0} {stabilityInfo}";
                rawBeatPulse.TriggerPulse();
                if (audioAnalysis.BeatStability >= stabilityThreshold)
                {
                    clapCatAnimator.SetTrigger(Clap);
                    stableBeatPulse.TriggerPulse();
                }
            }
        }

        private float UpdateInterval => 1 / updateFrequency;
    }
}