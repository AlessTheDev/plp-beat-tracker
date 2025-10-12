using System;
using Graphs;
using Graphs.Processors;
using UnityEngine;

namespace Visualizers
{
    public class Test : MonoBehaviour
    {
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private SmoothLineGraph lineGraph;

        private float[] _samples = new float[1024];
        private void Update()
        {
            audioSource.GetOutputData(_samples, 0);
            lineGraph.Process(_samples);
        }
    }
}