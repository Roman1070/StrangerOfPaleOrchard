using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OtherPlayerWidgetView : View
{
    [SerializeField]
    private Text _level;
    [SerializeField]
    private Image _levelProgress;
    [SerializeField]
    private Image _healthBar;
    [SerializeField]
    private Text _name;

    private PlayerView _target;
    private Camera _camera;
    private UpdateProvider _updateProvider;

    public void Init(Camera camera, UpdateProvider updateProvider, PlayerView target,string name, int level, float normalizedProgress)
    {
        _target = target;
        _camera = camera;
        _name.text = name;
        _updateProvider = updateProvider;
        _level.text = level.ToString();
        _levelProgress.fillAmount = normalizedProgress;
        _healthBar.fillAmount = 1;
        updateProvider.Updates.Add(LocalUpdate);
    }

    private void LocalUpdate()
    {
        if (_target != null) transform.position = _camera.WorldToScreenPoint(_target.transform.position + Vector3.up * 3);
        else 
        {
            _updateProvider.Updates.Remove(LocalUpdate);
            Destroy(gameObject); 
        }
    }

    public void UpdateHealth(float normalizedHealth) => _healthBar.fillAmount = normalizedHealth;
    public void UpdateLevelProgress(float normalizedExp) => _levelProgress.fillAmount = normalizedExp;
    public void UpdateLevel(int level) => _level.text = level.ToString();
}
