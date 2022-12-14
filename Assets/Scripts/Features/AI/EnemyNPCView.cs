using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Zenject;

public class EnemyNPCView : NPCViewBase, IDamagable, IPlayerTarget
{
    [Inject] private UpdateProvider _updateProvider;

    protected override NPCType NPCType => NPCType.Enemy;

    public Transform Target => transform;
    public int DamagableId => 1;

    private Dictionary<NPCState, bool> _states = new Dictionary<NPCState, bool>()
    {
        {NPCState.Chasing, false },
        {NPCState.Patroling,true },
        {NPCState.Attacking,false },
        {NPCState.Dying,false }
    };

    [SerializeField]
    private NavMeshAgent _navMeshAgent;
    [SerializeField]
    private AgentLinkMover _agentLinkMover;
    [SerializeField]
    private Animator _animator;
    [SerializeField]
    private EnemyNPCConfig _config;
    [SerializeField]
    private PhotonView _photonView;
    [SerializeField]
    private int _level;

    private Vector3 _startPoint;
    private NPCAttackData _currentAttack;
    private Vector3 _destinationPoint;
    private IDamagable _target;
    private Coroutine _attackRoutine;
    [SerializeField]
    private float _health;

    public EnemyNPCConfig Config => _config;
    public int Level =>_level;

    private float DistanceToTarget => Vector3.Distance(transform.position, _target.Transform.position);

    public float Health { get => _health; private set => _health = value; }
    private int Damage => _config.LevelData[_level - 1].Damage;

    public Transform Transform => transform;

    public bool IsAlive => Health > 0;


    private void Start()
    {
        Health = _config.LevelData[_level - 1].MaxHealth;
        _updateProvider.Updates.Add(LocalUpdate);
        _startPoint = transform.position;
        _destinationPoint = GetRandomDestinationPoint();
        _navMeshAgent.SetDestination(_destinationPoint);
        _agentLinkMover.OnLinkStart += HandleLinkStart;
    }

    private void HandleLinkStart()
    {
        _animator.SetTrigger("Jump");
    }

    private void LocalUpdate()
    {
        if (Health <= 0) return;

        UpdateState();
    }

    private void UpdateTarget(IDamagable target)
    {
        int id = target != null ? target.Transform.GetComponent<PhotonView>().ViewID : -1;
        UpdateTargetAction(id);
        _photonView.RPC("UpdateTargetRemote", RpcTarget.Others, id);
    }

    private void UpdateTargetAction(int id)
    {
        if (id != -1) _target = PhotonView.Find(id).GetComponent<IDamagable>();
        else _target = null;
    }
    [PunRPC]
    private void UpdateTargetRemote(int id)
    {
        UpdateTargetAction(id);
    }

    private void UpdateState()
    {
        var collidersInDetectionRange = Physics.OverlapSphere(transform.position, _config.DetectionDistance, LayerMask.GetMask("Player"));
        if (collidersInDetectionRange.Length > 0)
        {
            if (_target == null)
            {
                UpdateTarget(collidersInDetectionRange[0].transform.GetComponent<IDamagable>());
            }
        }
        else
        {
            if (_target != null) UpdateTarget(null);
        }

        if (_target != null)
        {
            if (DistanceToTarget <= _config.AttackRange)
            {
                if (!_states[NPCState.Attacking])
                {
                    _animator.SetLayerWeightSmooth(this, "CombatLayer", true, 8);
                    _attackRoutine = StartCoroutine(Attack());
                }
            }
            else if (DistanceToTarget > _config.AttackRange && DistanceToTarget < _config.ChaseDistance)
            {
                if (_states[NPCState.Attacking])
                {
                    _currentAttack = null;
                    _states[NPCState.Attacking] = false;
                    _animator.SetTrigger("exit combat");
                    _animator.SetLayerWeightSmooth(this, "CombatLayer", false, 8);
                    StopCoroutine(_attackRoutine);
                }
                Chase();
            }
            else Patrol();
        }
    }

    private IEnumerator Attack()
    {
        if (Health <= 0) yield return null;

        _states[NPCState.Attacking] = true;

        _currentAttack = _config.GetRandomAttack();
        _animator.SetTrigger(_currentAttack.Id);
        _target.TakeDamage(Damage);

        while (_target != null && DistanceToTarget <= _config.AttackRange)
        {
            if (Health <= 0) yield return null;
            yield return new WaitForSeconds(_currentAttack.Duration);
            _currentAttack = _config.GetRandomAttack(_currentAttack.Id);
            _animator.SetTrigger(_currentAttack.Id);
            _target.TakeDamage(Damage);
        }
    }

    private void Chase()
    {
        _states[NPCState.Patroling] = false;
        _states[NPCState.Chasing] = true;
        _animator.SetFloat("Speed", 2, 0.15f, Time.deltaTime);
        _destinationPoint = _target.Transform.position;

        if (Vector3.Distance(transform.position, _destinationPoint) < _navMeshAgent.stoppingDistance)
        {
            _navMeshAgent.isStopped = true;
            _navMeshAgent.velocity = Vector3.zero;
        }
        else if (Vector3.Distance(transform.position, _destinationPoint) > _navMeshAgent.stoppingDistance + 0.5f)
        {
            _navMeshAgent.isStopped = false;
            _navMeshAgent.SetDestination(_destinationPoint);
            _navMeshAgent.speed = _config.ChaseSpeed;
            if (Physics.Raycast(transform.position + Vector3.up, _target.Transform.position - transform.position, out var hit, 10))
            {
                if (hit.collider.TryGetComponent<PlayerView>(out var player))
                    _navMeshAgent.velocity = (player.transform.position - transform.position).normalized * _config.ChaseSpeed;
            }
        }
        transform.LookAt(_target.Transform);

    }
    private void Patrol()
    {
        _states[NPCState.Patroling] = true;
        _states[NPCState.Chasing] = false;
        _animator.SetFloat("Speed", 1, 0.15f, Time.deltaTime);
        _navMeshAgent.isStopped = false;
        _navMeshAgent.speed = _config.PatrolSpeed;
        if ((transform.position - _destinationPoint).magnitude < 1)
        {
            _destinationPoint = GetRandomDestinationPoint();
            _navMeshAgent.SetDestination(_destinationPoint);
        }
    }

    private Vector3 GetRandomDestinationPoint()
    {
        float z = Random.Range(_startPoint.z, _startPoint.z + _config.PatrolingDistance);
        float x = Random.Range(_startPoint.x, _startPoint.x + _config.PatrolingDistance);

        return new Vector3(x, transform.position.y, z);
    }

    private void Die()
    {
        _animator.SetTrigger(StringConst.ExitCombat);
        _animator.SetTrigger("dying");
        _states[NPCState.Dying] = true;
        _states[NPCState.Attacking] = false;
        _states[NPCState.Chasing] = false;
        _states[NPCState.Patroling] = false;
        _navMeshAgent.velocity = Vector3.zero;
        _navMeshAgent.isStopped = true;
        _animator.SetLayerWeight(_animator.GetLayerIndex("CombatLayer"), 0);
        GetComponent<Collider>().enabled = false;
        enabled = false;

        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
        SendOptions sendOptions = new SendOptions() { DeliveryMode = DeliveryMode.Reliable };
        PhotonNetwork.RaiseEvent((int)PhotonEventsCodes.OnEnemyDied, _photonView.ViewID, raiseEventOptions, sendOptions);
    }


    public void TakeDamage(int damage)
    {
        _photonView.RPC("TakeDamageRemote", RpcTarget.All, damage);
    }

    [PunRPC]
    private void TakeDamageRemote(int damage)
    {
        Health -= damage;

        if (Health <= 0)
        {
            Die();
        }
    }
}
