using System;
using UnityEngine;

namespace Graphs.Processors
{
    /// <summary>
    /// Simple graph where values can only be heigh or low
    /// </summary>
    public class BinaryGraph : GraphProcessor<int[]>
    {
        [SerializeField] private LineRenderer lineRenderer;

        /// <summary>
        /// Processes an array of integer values and updates the LineRenderer to display
        /// vertical binary lines (high/low) at the specified positions.
        /// </summary>
        /// <param name="upValues">Array of X positions (as indices) where the graph should be high.</param>
        public override void Process(int[] upValues)
        {
            if (Graph.XRange.Max == 0)
            {
                Debug.LogError("Set an XRange.Max Value");
            }

            Array.Sort(upValues);
            lineRenderer.positionCount = upValues.Length * 2;

            for (int i = 0; i < lineRenderer.positionCount; i++)
            {
                lineRenderer.SetPosition(i, Vector2.zero);
            }

            float xMax = Graph.dataContainer.rect.width;
            float xStep = xMax / (Graph.XRange.Max - 1);
            for (int i = 0; i < upValues.Length; i++)
            {
                float xPos = upValues[i] * xStep;
                int baseIdx = i * 2;
                lineRenderer.SetPosition(baseIdx, new Vector2(xPos, Graph.dataContainer.rect.height));
                lineRenderer.SetPosition(baseIdx + 1, new Vector2(xPos + 0.001f, 0));
            }
        }
    }
}