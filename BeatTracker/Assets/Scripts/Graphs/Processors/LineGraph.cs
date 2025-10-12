using System.Linq;
using UnityEngine;

namespace Graphs.Processors
{
    /// <summary>
    /// A graph processor that renders a continuous line graph from an array of float values.
    /// </summary>
    public class LineGraph : GraphProcessor<float[]>
    {
        [SerializeField] private LineRenderer lineRenderer;

        private bool _useDefaultRange = true;

        public override void Process(float[] input)
        {
            if(input.Length < 2) Debug.LogError($"Input length is less than 2!, ({gameObject.name})");
            
            if (_useDefaultRange)
            {
                Graph.XRange.SetRange(0, input.Length);
            }

            float min = input.Min();
            float max = input.Max();
            
            float lowestValue = Graph.YRange.Min;
            float highestValue = Graph.YRange.Max;

            if (min < lowestValue)
            {
                lowestValue = min;
            }

            if (max > highestValue)
            {
                highestValue = max;
            }

            Graph.YRange.SetRange(lowestValue, highestValue);

            lineRenderer.positionCount = input.Length;

            float xMax = Graph.dataContainer.rect.width;
            float yMax = Graph.dataContainer.rect.height;

            float xUnit = xMax / (input.Length - 1);
            float range = Graph.YRange;
            
            if(range == 0) return;

            for (int i = 0; i < input.Length; i++)
            {
                float normalizedValue = (input[i] - Graph.YRange.Min) / range;
                lineRenderer.SetPosition(i, new Vector3(xUnit * i, normalizedValue * yMax, 0));
            }
        }

        public void SetRangeOverride(float min, float max)
        {
            _useDefaultRange = false;
            Graph.XRange.SetRange(min, max);
        }
    }
}