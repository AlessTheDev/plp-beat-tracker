using System;
using AudioAnalysis;
using UnityEngine;
using UnityEngine.UI;

namespace Graphs.Processors
{
    /// <summary>
    /// A graph processor that visualizes streaming color data as a scrolling texture.
    /// </summary>
    public class ScrollingTextureGraph : GraphProcessor<Color32[]>
    {
        [SerializeField] private int windowSize;
        [SerializeField] private Image image;

        private Texture2D _texture;
        private CirclularBuffer<Color32[]> _buffer;
        private Sprite _sprite;

        private Color32[] _cache;

        private void Start()
        {
            _buffer = new CirclularBuffer<Color32[]>(windowSize);
        }

        /// <summary>
        /// Creates and initializes the texture and sprite used for rendering the graph.
        /// </summary>
        /// <param name="height">The height of the texture in pixels.</param>
        public void InitializeTexture(int height)
        {
            _texture = new Texture2D(windowSize, height, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point // Disable filtering for performance
            };

            _cache = new Color32[_texture.width * _texture.height];
            _sprite = Sprite.Create(_texture, 
                new Rect(0, 0, windowSize, height), 
                Vector2.one * 0.5f);
            image.sprite = _sprite;
        }

        /// <summary>
        /// Processes an array of colors representing one frame of input data,
        /// updates the buffer, and redraws the scrolling texture.
        /// </summary>
        /// <param name="colors">An array of color values for the current frame.</param>
        public override void Process(Color32[] colors)
        {
            if (_buffer.Head == 0)
            {
                Graph.XRange.SetRange(-Time.deltaTime * windowSize, 0);
            }

            _buffer.Add(colors);

            int bufferLength = _buffer.Length;
            int frameLength = colors.Length;
            
            for (int i = 0; i < bufferLength; i++)
            {
                var frame = _buffer.GetFIFO(i);
                if (frame == null) continue;

                for (int k = 0; k < frameLength; k++)
                {
                    _cache[k * bufferLength + i] = frame[k];
                }
            }
            
            _texture.SetPixels32(_cache);
            _texture.Apply(false);
        }
    }
}