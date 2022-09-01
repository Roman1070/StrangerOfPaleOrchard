using DG.Tweening;
using System;
using System.Collections;
using UnityEngine;

public class PlayerArmedStateController : PlayerGearControllerBase
{
    private float _animationDuration = 0.6f;
    private Animator _animator;
    private PlayerStatesService _statesService;
    private Transform _handAnchor;
    private Transform _spineAnchor;
    private Transform _weaponHolder;

    private Vector3 DrawnPosition => new Vector3(-0.007f, 0.067f , 0);
    private Vector3 RemovedPosition => new Vector3(-0.188f, 0.325f , -0.06f);
    private Vector3 RemovedRotation => new Vector3(103, 94 , 18);

    public PlayerArmedStateController(SignalBus signalBus, PlayerView player, PlayerStatesService statesService, EquipedWeaponOffsetConfig weaponOffsetConfig) : base(signalBus, player)
    {
        _statesService = statesService;
        _handAnchor = player.HandAnchor;
        _spineAnchor = player.SpineAnchor;
        _weaponHolder = player.WeaponsHolder;
        _animator = _player.Model.GetComponent<Animator>();

        signalBus.Subscribe<DrawWeaponSignal>(DrawWeapon, this);
        signalBus.Subscribe<DrawWeaponRemoteSignal>(DrawWeaponRemote, this);
        signalBus.Subscribe<ToggleWeaponDrawnStatusSignal>(ToggleArmedStatus, this);

        _weaponHolder.SetParent(_spineAnchor);
        _weaponHolder.transform.localPosition = RemovedPosition;
        _weaponHolder.transform.localEulerAngles = RemovedRotation;
    }

    private void DrawWeapon(DrawWeaponSignal signal)
    {
        if (signal.Draw)_player.StartCoroutine(DrawWeapon());
        else _player.StartCoroutine(RemoveWeapon());
    }

    private void ToggleArmedStatus(ToggleWeaponDrawnStatusSignal signal)
    {
        if (_statesService.States[PlayerState.IsArmed]) _player.StartCoroutine(RemoveWeapon());
        else _player.StartCoroutine(DrawWeapon());
        
    }

    private IEnumerator DrawWeapon()
    {
        yield return new WaitUntil(() => !_statesService.States[PlayerState.DrawingWeapon] && !_statesService.States[PlayerState.Attacking]);

        _animator.SetTrigger("DrawWeapon");
        _player.Photon.RPC("DrawWeaponRemote", Photon.Pun.RpcTarget.Others, true);
        _signalBus.FireSignal(new SetPlayerStateSignal(PlayerState.DrawingWeapon, true));

        DOVirtual.DelayedCall(_animationDuration * 0.35f, () =>
        {
            _weaponHolder.SetParent(_handAnchor);
            _weaponHolder.transform.localPosition = DrawnPosition;
            _weaponHolder.transform.localEulerAngles = Vector3.zero;
        });

        DOVirtual.DelayedCall(_animationDuration + 0.15f, () =>
        {
            _animator.SetBool("IsArmed", true);
            _signalBus.FireSignal(new SetPlayerStateSignal(PlayerState.IsArmed, true));
            _signalBus.FireSignal(new SetPlayerStateSignal(PlayerState.DrawingWeapon, false));
        }); 
    }

    private void DrawWeaponRemote(DrawWeaponRemoteSignal signal)
    {
        float delay = signal.Draw ? _animationDuration * 0.35f : _animationDuration + 0.2f;
        signal.Player.Model.GetComponent<Animator>().SetBool("IsArmed", signal.Draw);
        DOVirtual.DelayedCall(delay, () =>
        {
            signal.Player.WeaponsHolder.SetParent(signal.Draw ? signal.Player.HandAnchor : signal.Player.SpineAnchor);
        });
    }

    private IEnumerator RemoveWeapon()
    {
        yield return new WaitUntil(() => !_statesService.States[PlayerState.DrawingWeapon] && !_statesService.States[PlayerState.Attacking]);

        _animator.SetTrigger("RemoveWeapon");
        _player.Photon.RPC("DrawWeaponRemote", Photon.Pun.RpcTarget.Others, false);
        _signalBus.FireSignal(new SetPlayerStateSignal(PlayerState.DrawingWeapon, true));
        DOVirtual.DelayedCall(_animationDuration+0.2f, () =>
        {
            _weaponHolder.SetParent(_spineAnchor);
            _weaponHolder.transform.DOLocalMove(RemovedPosition, 0.2f);
            _weaponHolder.transform.localEulerAngles = RemovedRotation;

            DOVirtual.DelayedCall(0.15f, () =>
            {
                _animator.SetBool("IsArmed", false);
                _signalBus.FireSignal(new SetPlayerStateSignal(PlayerState.DrawingWeapon, false));
                _signalBus.FireSignal(new SetPlayerStateSignal(PlayerState.IsArmed, false));
            });
        });
    }
}
