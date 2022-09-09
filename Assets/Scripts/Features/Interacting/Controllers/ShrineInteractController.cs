﻿using DG.Tweening;
using System;
using System.Collections;
using UnityEngine;

public class ShrineInteractController : ItemInteractControllerBase
{
    private PlayerStatesService _statesService;

    public ShrineInteractController(SignalBus signalBus, PlayerView player, PlayerStatesService statesService) : base(signalBus, player)
    {
        _statesService = statesService;
    }

    protected override void Interact(InteractableObject obj)
    {
        if (obj is InteractableShrine)
        {
            if ((_statesService.States[PlayerState.IsArmed]&& !_statesService.States[PlayerState.DrawingWeapon]) 
                || (_statesService.States[PlayerState.DrawingWeapon]&& ! _statesService.States[PlayerState.IsArmed]))
            {
                _signalBus.FireSignal(new DrawWeaponSignal(false,_player));
            }
            _player.StartCoroutine(Pray(obj as InteractableShrine));
        }
    }

    private IEnumerator Pray(InteractableShrine obj)
    {
        if (_statesService.States[PlayerState.DrawingWeapon] || _statesService.States[PlayerState.IsArmed]) 
            yield return new WaitUntil(()=>!_statesService.States[PlayerState.DrawingWeapon] && !_statesService.States[PlayerState.IsArmed]);

        _animator.SetTrigger(StringConst.PRAY);
        _player.Photon.RPC("SetRemoteInteractingTrigger", Photon.Pun.RpcTarget.Others, StringConst.PRAY);
        _player.StartCoroutine(InteractProcess(obj));
    }

    protected override void OnCollected(InteractableObject obj)
    {
        (obj as InteractableShrine).VFX.loop = false;
        (obj as InteractableShrine).VFX.DOPlayBackwards();
        _signalBus.FireSignal(new OnExperienceChangedSignal((obj as InteractableShrine).ExperienceAmount, _player));
    }
}
