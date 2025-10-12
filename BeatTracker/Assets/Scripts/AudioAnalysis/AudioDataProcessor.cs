using System;

namespace AudioAnalysis
{
    /// <summary>
    /// Processes data from the OnAudioFilterRead by combining channels and then
    /// invokes a callback when the samples are ready to be processed
    /// </summary>
    public class AudioDataProcessor
    {
        private readonly int _hopSize;
        private readonly Action<float[]> _onSamplesReady;

        private readonly CirclularBuffer<float> _audioBuffer;

        /// <summary>
        /// Initializes a new instance of the <see cref="AudioDataProcessor"/> class.
        /// </summary>
        /// <param name="desiredSamples">
        /// The total number of audio samples to hold in the internal buffer. This defines
        /// the buffer length used for sample accumulation before invoking the callback.
        /// Therefore, it's also the length of the data that gets sent when invoking the callback.
        /// </param>
        /// <param name="hopSize">
        /// The interval (in samples) at which the processor triggers the <paramref name="onSamplesReady"/> 
        /// callback. 
        /// </param>
        /// <param name="onSamplesReady">
        /// A callback that is invoked whenever the buffer has been filled at the defined hop interval. 
        /// The callback receives the buffer contents as a float array, with audio channels combined into one signal.
        /// </param>
        public AudioDataProcessor(int desiredSamples, int hopSize, Action<float[]> onSamplesReady)
        {
            _audioBuffer = new CirclularBuffer<float>(desiredSamples);
            _hopSize = hopSize;
            _onSamplesReady = onSamplesReady;
        }

        public void Process(float[] data, int channels)
        {
            // Number of samples with channels combined
            int dataSamples = data.Length / channels; 
            
            for (int k = 0; k < dataSamples; k++)
            {
                // Combine channels
                float sample = 0;
                for (int j = 0; j < channels; j++)
                {
                    sample += data[k * channels + j];
                }
                sample /= channels;

                _audioBuffer.Add(sample);

                // Check if the samples are ready
                if (_audioBuffer.Head % _hopSize == 0 && _audioBuffer.HasBeenFilled)
                {
                    _onSamplesReady.Invoke(_audioBuffer.ToOrderedArray());
                }
            }
        }
    }
}