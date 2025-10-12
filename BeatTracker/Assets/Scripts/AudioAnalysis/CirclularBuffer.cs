using System;

namespace AudioAnalysis
{
    /// <summary>
    /// A generic circular buffer (ring buffer) implementation.
    /// Stores a fixed number of elements and overwrites the oldest data when full.
    /// </summary>
    /// <typeparam name="T">The type of elements stored in the buffer.</typeparam>
    public class CirclularBuffer<T>
    {
        public T[] Values { get; private set; } // Raw array, allocated once, and used to store values
        public int Head { get; private set; } // Pointer index where the next element will be written.
        
        /// <summary>
        /// Indicates whether the buffer has been filled at least once.
        /// </summary>
        public bool HasBeenFilled { get; private set; }

        /// <summary>
        /// Initializes a new empty buffer with a specified size.
        /// </summary>
        /// <param name="size">The fixed capacity of the buffer.</param>
        public CirclularBuffer(int size)
        {
            Values = new T[size];
            Head = 0;
            
            HasBeenFilled = false;
        }
        
        public CirclularBuffer(T[] values)
        {
            Values = new T[values.Length];
            Array.Copy(values, Values, values.Length);
            Head = 0;
            
            HasBeenFilled = false;
        }

        public void Add(T value)
        {
            Values[Head] = value;
            Head = (Head + 1) % Values.Length;
            
            if(Head == 0) HasBeenFilled = true; // The buffer completed a cycle
        }

        public void Add(T[] values)
        {
            foreach (var value in values)
            {
                Add(value);
            }
        }
        
        /// <summary>
        /// Adds the contents of another circular buffer to this buffer in FIFO order.
        /// </summary>
        /// <param name="values">The circular buffer whose values will be added</param>
        public void Add(CirclularBuffer<T> values)
        {
            for (int i = 0; i < values.Length; i++)
            {
                Add(values.GetFIFO(i));
            }
        }

        public int Length => Values.Length;
        
        /// <summary>
        /// Gets the raw value at the specified index (not in FIFO order).
        /// </summary>
        /// <param name="i">The array index.</param>
        /// <returns>The value at the specified index.</returns>
        public T GetRaw(int i) => Values[i];
        
        // ReSharper disable twice InconsistentNaming
        
        /// <summary>
        /// Gets the value at the specified position in FIFO order.
        /// </summary>
        /// <param name="i">The position in FIFO order.</param>
        /// <returns>The value at the specified FIFO index.</returns>
        public T GetFIFO(int i) => Values[GetIndexFIFO(i)];

        /// <summary>
        /// Gets the underlying array index corresponding to a FIFO index.
        /// </summary>
        /// <param name="i">The FIFO index.</param>
        /// <returns>The array index in <see cref="Values"/>.</returns>
        public int GetIndexFIFO(int i) => HasBeenFilled ? (Head + i) % Values.Length : i;

        /// <summary>
        /// Returns a copy of the buffer contents in FIFO order.
        /// </summary>
        /// <returns>
        /// A new array containing the buffer elements ordered from oldest to newest.
        /// </returns>
        public T[] ToOrderedArray()
        {
            if (Head == 0) return Values;
            
            T[] orderedValues = new T[Length];
            for (int i = 0; i < Length; i++) orderedValues[i] = GetFIFO(i);
            return orderedValues;
        }
    }
}