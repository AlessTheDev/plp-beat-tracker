using System;
using TMPro;
using UnityEngine;

public class FPSVisual : MonoBehaviour
{
    private TextMeshProUGUI _text;


    private void Start()
    {
        _text = GetComponent<TextMeshProUGUI>();
    }

    private void Update()
    {
        _text.text = "FPS: " + Mathf.Round(1 / Time.deltaTime);
    }
}