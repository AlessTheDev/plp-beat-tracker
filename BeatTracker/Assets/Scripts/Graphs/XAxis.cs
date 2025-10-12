using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Graphs
{
    public class XAxis : GraphComponent
    {
        [SerializeField] private RectTransform labelsContainer;
        [SerializeField] private TextMeshProUGUI labelPrefab;
        [SerializeField] private int labelsCount = 3;
        [SerializeField] private float labelsWidth = .3f;
        [SerializeField] private bool displayDecimalsInLabels = false;
        
        private List<TextMeshProUGUI> _labelsCache;

        private void Start()
        {
            _labelsCache = new List<TextMeshProUGUI>();
            Graph.XRange.OnValuesChange.AddListener(UpdateAxis);
        }

        private void UpdateAxis(ValueRange range)
        {
            int intervals = labelsCount - 1;
            
            // Add necessary labels if needed
            for (int i = _labelsCache.Count; i < labelsCount; i++)
            {
                TextMeshProUGUI newLabel = Instantiate(labelPrefab, labelsContainer);
                _labelsCache.Add(newLabel);
            }

            for (int i = 0; i < _labelsCache.Count; i++)
            {
                var label = _labelsCache[i];
                if (i < labelsCount)
                {
                    label.gameObject.SetActive(true);

                    float t = (float)i / intervals; // 0..1
                    float value = Mathf.Lerp(range.Min, range.Max, t);
                    label.text = value.ToString(displayDecimalsInLabels ? "0.#" : null);

                    float xPosition = (labelsContainer.rect.width / intervals) * i;
                    
                    label.rectTransform.anchoredPosition = new Vector2(xPosition, 0);
                    label.rectTransform.anchorMin = Vector2.zero;
                    label.rectTransform.anchorMax = Vector2.up;
                    label.rectTransform.sizeDelta = new Vector2(labelsContainer.rect.width / intervals * labelsWidth, 0);
                }
                else
                {
                    label.gameObject.SetActive(false);
                }
            }
        }
    }
}