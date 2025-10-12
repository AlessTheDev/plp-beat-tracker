using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Graphs
{
    public class YAxis : GraphComponent
    {
        [SerializeField] private RectTransform labelsContainer;
        [SerializeField] private TextMeshProUGUI labelPrefab;
        [SerializeField] private int labelsCount = 3;
        [SerializeField] private float labelsHeight = .3f;
        
        private List<TextMeshProUGUI> _labelsCache;

        private void Start()
        {
            _labelsCache = new List<TextMeshProUGUI>();
            Graph.YRange.OnValuesChange.AddListener(UpdateAxis);
        }

        private void UpdateAxis(ValueRange range)
        {
            int intervals = labelsCount - 1;
            
            // Add labels if needed
            for (int i = _labelsCache.Count; i < labelsCount; i++)
            {
                TextMeshProUGUI newLabel = Instantiate(labelPrefab, labelsContainer);
                _labelsCache.Add(newLabel);
            }

            float containerHeight = labelsContainer.rect.height;

            for (int i = 0; i < _labelsCache.Count; i++)
            {
                var label = _labelsCache[i];
                if (i < labelsCount)
                {
                    label.gameObject.SetActive(true);

                    float t = (float)i / intervals; // 0..1
                    float value = Mathf.Lerp(range.Min, range.Max, t);
                    label.text = value.ToString("0.##");

                    // Position label in container
                    float yPosition = t * containerHeight;
                    label.rectTransform.anchoredPosition = new Vector2(0, yPosition);
                    
                    label.rectTransform.anchorMin = Vector2.zero;
                    label.rectTransform.anchorMax = Vector2.right;
                    label.rectTransform.sizeDelta = new Vector2(0, labelsContainer.rect.height / intervals * labelsHeight);
                }
                else
                {
                    label.gameObject.SetActive(false);
                }
            }
        }
    }
}