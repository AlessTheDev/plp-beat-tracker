using System;
using AudioAnalysis;
using UnityEngine;

namespace BeatTracking
{
    public class BufferTester : MonoBehaviour
    {
        private CirclularBuffer<float> _buffer;

        private void Start()
        {
            _buffer = new(3);
            _buffer.Add(1);
            _buffer.Add(2);
            _buffer.Add(3);
            _buffer.Add(4);

            for (int i = 0; i < _buffer.Length; i++)
            {
                Debug.Log("Buffet raw i: " + _buffer.GetRaw(i));
            }
            
            for (int i = 0; i < _buffer.Length; i++)
            {
                Debug.Log("Buffet FIFO i: " + _buffer.GetFIFO(i));
            }
        }
    }
}