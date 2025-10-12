using System;
using AudioAnalysis;
using UnityEngine;

namespace Graphs.Processors
{
    /// <summary>
    /// A graph processor that displays continuously updating data as a scrolling line graph.
    /// </summary>
    public class ScrollingLineGraph : GraphProcessor<float>
    {
        [SerializeField] private LineRenderer lineRenderer;
        [SerializeField] private int windowSize;
        private CirclularBuffer<float> _buffer;

        private float _lowestValue;
        private float _highestValue;

        private void Start()
        {
            _buffer = new CirclularBuffer<float>(windowSize);
            lineRenderer.positionCount = windowSize;
            
            _lowestValue = Graph.YRange.Min;
            _highestValue = Graph.YRange.Max;
        }

        public void Process(float[] frame)
        {
            if(frame == null) return;
            
            for (int i = 0; i < frame.Length; i++)
            {
                Process(frame[i]);
            }
        }

        public override void Process(float value)
        {
            if (value < _lowestValue)
            {
                _lowestValue = value;
            }

            if (value > _highestValue)
            {
                _highestValue = value;
            }

            if (_buffer.Head == 0)
            {
                Graph.XRange.SetRange(-Time.deltaTime * windowSize, 0);
            }
            
            Graph.YRange.SetRange(_lowestValue, _highestValue);
            
            _buffer.Add(value);
            
            float xMax = Graph.dataContainer.rect.width;
            float yMax = Graph.dataContainer.rect.height;

            float xUnit = xMax / (windowSize - 1);
            float range = Graph.YRange;

            if (range == 0) return;
            
            for (int i = 0; i < windowSize; i++)
            {
                float normalizedValue = (_buffer.GetFIFO(i) - Graph.YRange.Min) / range;
                lineRenderer.SetPosition(i, new Vector3(xUnit * i, normalizedValue * yMax, 0));
            }
        }
    }
}
