using UnityEngine;
using UnityEngine.UI;

public class InteractButton : View
{
    [SerializeField] private CanvasGroup _canvasGroup;
    [SerializeField] private Text _lowerLabel;

    public Button Button;

    private void Start()
    {
        SetActive(false);
    }
    public void SetData(string lowerLabel)
    {
        _lowerLabel.text = lowerLabel;
    }
    public void SetActive(bool val)
    {
        _canvasGroup.alpha = val ? 1 : 0;
        _canvasGroup.blocksRaycasts = val;
    }
}
