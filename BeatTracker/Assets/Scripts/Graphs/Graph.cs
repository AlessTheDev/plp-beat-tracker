using System;
using UnityEngine;
using UnityEngine.Events;

namespace Graphs
{
    public class ValueRange
    {
        public UnityEvent<ValueRange> OnValuesChange = new();

        public float Min { get; private set; } = float.MaxValue;
        public float Max { get; private set; } = float.MinValue;

        public void SetRange(float min, float max)
        {
            // ReSharper disable twice CompareOfFloatsByEqualityOperator
            if (min == Min && max == Max) return;

            Min = min;
            Max = max;
            OnValuesChange.Invoke(this);
        }

        public static implicit operator float(ValueRange value) => value.Max - value.Min;
    }

    public class Graph : MonoBehaviour
    {
        public readonly ValueRange XRange = new();
        public readonly ValueRange YRange = new();
        public RectTransform dataContainer;
    }
}