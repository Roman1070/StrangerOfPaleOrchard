using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;

public class PlayerCombatService : LoadableService, IOnEventCallback
{
    private UpdateProvider _updateProvider;
    private Camera _camera;
    private IDamagable _target;
    private PlayerView _player;
    private PlayerCombatConfig _config;
    private CoroutineExecutor _executor;
    private PlayerStatesService _states;
    private PlayerAttackData _currentAttack;
    private PlayerDataService _dataService;
    private Animator _animator;
    private InventoryService _inventory;
    private AttackType _attackType;
    private Coroutine _attackRoutine;
    private List<int> _recentTargetsPhotonIds = new List<int>();
    private int _targetPhotonIndex => _target.Transform.GetComponent<PhotonView>().ViewID;

    private float DistanceToTarget => Vector3.Distance(_player.transform.position, _target.Transform.position);

    public PlayerCombatService(SignalBus signalBus, UpdateProvider updateProvider, Camera camera, PlayerView player, PlayerCombatConfig config, CoroutineExecutor executor) : base(signalBus)
    {
        _updateProvider = updateProvider;
        _camera = camera;
        _player = player;
        _config = config;
        _executor = executor;
        _animator = _player.Model.GetComponent<Animator>();
        PhotonNetwork.AddCallbackTarget(this);
    }

    private void Update()
    {
        CheckInput();
        if (_target!=null && DistanceToTarget <= _config.AttackRange)
        {
            if (!_states.States[PlayerState.Attacking])  _attackRoutine= _executor.StartCoroutine(Attack());
        }
        else
        {
            if (_attackRoutine != null && _states.States[PlayerState.Attacking])
            {
                InterruptAttack();
            }
        }
    }

    private void InterruptAttack()
    {
        _target = null;
        _executor.StopCoroutine(_attackRoutine);
        _states.States[PlayerState.Attacking] = false;
        _player.Photon.RPC("SendRemoteCombatTrigger", RpcTarget.Others, StringConst.ExitCombat);
        _animator.SetTrigger(StringConst.ExitCombat);
        _animator.SetLayerWeightSmooth(_executor, PlayerCombatConfig.LayersMappings[_attackType], false, 4);
    }

    private IEnumerator Attack()
    {
        if (_dataService.DynamicData.Health <= 0) yield return null;

        _states.States[PlayerState.Attacking] = true;

        Vector3 direction = (_target.Transform.position - _player.transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(direction);
        _player.transform.DORotate(lookRotation.eulerAngles, 0.2f);

        _attackType = _states.States[PlayerState.IsArmed] ? (AttackType)_inventory.Inventory.CurrentWeaponType : AttackType.Disarmed;
        _currentAttack = _config.GetRandomAttack("", _attackType);
        _animator.SetLayerWeightSmooth(_executor,PlayerCombatConfig.LayersMappings[_attackType],true,4);
        _animator.SetTrigger(_currentAttack.Id);

        _target.TakeDamage(10);
        if (!_recentTargetsPhotonIds.Contains(_targetPhotonIndex)) _recentTargetsPhotonIds.Add(_targetPhotonIndex);

        while (_target != null && DistanceToTarget <= _config.AttackRange &&_target.IsAlive)
        {
            if (_dataService.DynamicData.Health <= 0) yield return null;
            yield return new WaitForSeconds(_currentAttack.Duration);
            _currentAttack = _config.GetRandomAttack(_currentAttack.Id, _attackType);
            _animator.SetTrigger(_currentAttack.Id);
            _target.TakeDamage(10);
            _player.Photon.RPC("SendRemoteCombatTrigger", RpcTarget.Others, _currentAttack.Id);
            _player.transform.LookAt(_target.Transform);
            if (!_target.IsAlive)
            {
                InterruptAttack();
            }
        }
    }

    private void CheckInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (Physics.Raycast(_camera.ScreenPointToRay(Input.mousePosition), out var hit))
            {
                if (hit.collider.TryGetComponent(out IDamagable target))
                {
                    if(target.Transform.GetComponent<PlayerView>()==null)
                    _target = target;
                }
                else
                {
                    _target = null;
                    if (_attackRoutine != null &&_states.States[PlayerState.Attacking]) InterruptAttack();
                }
            }
        }
    }

    public override void OnServicesLoaded(params LoadableService[] services)
    {
        _states = services.FirstOrDefault(s => s is PlayerStatesService) as PlayerStatesService;
        _dataService = services.FirstOrDefault(s => s is PlayerDataService) as PlayerDataService;
        _inventory = services.FirstOrDefault(s => s is InventoryService) as InventoryService;

        _updateProvider.Updates.Add(Update);
    }

    public void OnEvent(EventData photonEvent)
    {
        switch (photonEvent.Code)
        {
            case (int)PhotonEventsCodes.OnEnemyDied:
                int killedEnemyId = (int)photonEvent.CustomData;
                if (_recentTargetsPhotonIds.Contains(killedEnemyId))
                {
                    EnemyNPCView defeatedEnemy = PhotonView.Find(killedEnemyId).GetComponent<EnemyNPCView>();
                    int level = defeatedEnemy.Level;
                    int exp = defeatedEnemy.Config.LevelData[level - 1].ExpOnKill;
                    _signalBus.FireSignal(new OnExperienceChangedSignal(exp, _player));
                    _recentTargetsPhotonIds.Remove(killedEnemyId);
                }
                break;
        }
    }
}
