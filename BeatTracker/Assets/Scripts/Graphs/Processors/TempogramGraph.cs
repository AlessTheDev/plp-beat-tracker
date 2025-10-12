using System;
using System.Linq;
using System.Numerics;
using UnityEngine;

namespace Graphs.Processors
{
    [RequireComponent(typeof(ScrollingTextureGraph))]
    public class TempogramGraph : GraphProcessor<Complex[]>
    {
        [SerializeField] private Color lowValueColor = Color.white;
        [SerializeField] private Color highValueColor = Color.black;

        private ScrollingTextureGraph _textureGraph;

        private bool _hasBeenInitialized;

        /// <summary>
        /// Initializes the tempogram graph if it has not already been initialized.
        /// Sets the Y range based on the <paramref name="theta"/> array and creates a texture.
        /// </summary>
        /// <param name="frameLength">The vertical resolution (height) of each frame.</param>
        /// <param name="theta">The array of tempo frequencies or values for the Y axis.</param>
        public void InitializeIfNeeded(int frameLength, float[] theta)
        {
            if (_hasBeenInitialized) return;
            
            Graph.YRange.SetRange(theta.Min(), theta.Max());
            
            _textureGraph.InitializeTexture(frameLength);

            _hasBeenInitialized = true;
        }

        private void Start()
        {
            _hasBeenInitialized = false;
            _textureGraph = GetComponent<ScrollingTextureGraph>();
        }

        public override void Process(Complex[] activationFrame)
        {
            var output = new Color32[activationFrame.Length];

            double maxMagnitude = activationFrame.Max((c) => c.Magnitude);

            for (int i = 0; i < activationFrame.Length; i++)
            {
                double normMag = activationFrame[i].Magnitude / maxMagnitude;
                Color32 c = Color32.Lerp(lowValueColor, highValueColor, (float)normMag);
                output[i] = c;
            }

            _textureGraph.Process(output);
        }
    }
}