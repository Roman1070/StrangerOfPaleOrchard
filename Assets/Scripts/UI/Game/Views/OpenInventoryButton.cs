using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class OpenInventoryButton : MonoBehaviour
{
    [Inject]
    private SignalBus _signalBus;
    [SerializeField]
    private Button _button;

    private void Awake()
    {
        _button.onClick.RemoveAllListeners();
        _button.onClick.AddListener(() =>
        {
            _signalBus.FireSignal(new SetActivePanelSignal(typeof(InventoryPanel)));
        });
    }
}
