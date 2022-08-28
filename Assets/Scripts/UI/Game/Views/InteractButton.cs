﻿using UnityEngine;
using UnityEngine.UI;

public class InteractButton : View
{
    [SerializeField] private CanvasGroup _canvasGroup;
    [SerializeField] private Text _text;
    [SerializeField] private Text _lowerLabel;

    private void Start()
    {
        SetActive(false);
    }
    public void SetData(string text, string lowerLabel)
    {
        _text.text = text;
        _lowerLabel.text = lowerLabel;
    }
    public void SetActive(bool val)
    {
        _canvasGroup.alpha = val ? 1 : 0;
        _canvasGroup.blocksRaycasts = val;
    }
}
