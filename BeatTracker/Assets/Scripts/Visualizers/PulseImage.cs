using System;
using UnityEngine;
using UnityEngine.UI;

namespace Visualizers
{
    [RequireComponent(typeof(Image))]
    public class PulseImage : MonoBehaviour
    {
        [SerializeField] private float sizeIncreaseMultiplier = 1.1f;
        [SerializeField] private Color normalColor;
        [SerializeField] private Color highlightedColor;
        [SerializeField] private float fadeToNormalSpeed = 4;

        private Vector2 _initialSize;
        private Image _image;

        private bool _triggerPulse;

        private void Awake()
        {
            _image = GetComponent<Image>();
            _initialSize = _image.rectTransform.sizeDelta;
            _triggerPulse = false;
        }

        private void Update()
        {
            _image.color = Color.Lerp(_image.color, normalColor, fadeToNormalSpeed * Time.deltaTime);
            _image.rectTransform.sizeDelta = Vector2.Lerp(_image.rectTransform.sizeDelta, _initialSize,fadeToNormalSpeed * Time.deltaTime);

            if (_triggerPulse)
            {
                _image.color = highlightedColor;
                _image.rectTransform.sizeDelta = _initialSize * sizeIncreaseMultiplier;
                _triggerPulse = false;
            }
        }

        public void TriggerPulse()
        {
            _triggerPulse = true;
        }
    }
}