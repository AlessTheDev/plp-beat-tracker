using UnityEngine;

namespace Graphs.Processors
{
    /// <summary>
    /// <cref>LineGraph</cref> but the values are interpolated over time.
    /// </summary>
    public class SmoothLineGraph : LineGraph
    {
        [SerializeField] private float speed;
        
        private float[] _previousValues;
        
        public override void Process(float[] values)
        {
            if (_previousValues != null)
            {
                var lerpedValues = new float[values.Length];
                for (int i = 0; i < lerpedValues.Length; i++)
                {
                    float t = Mathf.Abs(values[i]) > Mathf.Abs(_previousValues[i]) ? speed * 15f : speed * 1f;
                    t *= Time.deltaTime;
                    lerpedValues[i] = Mathf.Lerp(_previousValues[i], values[i], t);
                }
                base.Process(lerpedValues);
                
                _previousValues = (float[])lerpedValues.Clone();
            }
            else if(values != null)
            {
                _previousValues = (float[])values.Clone();
                base.Process(values);
            }

        }
    }
}