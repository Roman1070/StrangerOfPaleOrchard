using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Zenject;

public class EnemyNPCView : NPCViewBase, IDamagable,IPlayerTarget
{
    [Inject] private UpdateProvider _updateProvider;

    protected override NPCType NPCType => NPCType.Enemy;

    public Transform Target => transform;

    private Dictionary<NPCState, bool> _states = new Dictionary<NPCState, bool>()
    {
        {NPCState.Chasing, false },
        {NPCState.Patroling,true },
        {NPCState.Attacking,false }
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
    private Rigidbody _rb;

    private Vector3 _startPoint;

    private float _distanceToPlayer;
    private Vector3 _destinationPoint;
    private PlayerView _target;
    private float _health;

    private void Start()
    {
        _health = _config.MaxHealth;
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
        if (_health <= 0) return;
        UpdateState();
    }

    private void UpdateState()
    {
        var collidersInDetectionRange = Physics.OverlapSphere(transform.position, _config.DetectionDistance, LayerMask.GetMask("Player"));
        if (collidersInDetectionRange.Length>0)
        {
            if (_target == null) _target = collidersInDetectionRange[0].transform.GetComponent<PlayerView>();
        }
        else
        {
            if(_target!=null)Debug.LogError(1);
            _target = null;
        }

        if (_target != null)
        {
            float distanceToTarget = Vector3.Distance(transform.position, _target.transform.position);
            if (distanceToTarget <= _config.AttackRange) Attack();
            else Chase();
        }
    }

    private void Attack()
    {
        if (_health <= 0) return;

        _states[NPCState.Attacking] = true;
        var attack = _config.Attacks.Random();
        _animator.SetTrigger(attack.Id);

        DOVirtual.DelayedCall(attack.Duration, () =>
        {
            if (_distanceToPlayer <= _config.AttackRange)
            {
                Attack();
            }
            else
            {
                _states[NPCState.Attacking] = false;
                _animator.SetTrigger("exit combat");
                Chase();
                _animator.SetLayerWeightSmooth(this, "MovementLayer", true, 8);
                _animator.SetLayerWeightSmooth(this, "CombatLayer", false, 8);
            }
        });
    }

    private void Chase()
    {
        _states[NPCState.Patroling] = false;
        _states[NPCState.Chasing] = true;
        _animator.SetFloat("Speed", 2, 0.15f, Time.deltaTime);
        _destinationPoint = _target.transform.position;

        if (Vector3.Distance(transform.position, _destinationPoint) < _navMeshAgent.stoppingDistance) 
        { 
            _navMeshAgent.isStopped = true;
            _navMeshAgent.velocity = Vector3.zero;
        }
        else if (Vector3.Distance(transform.position, _destinationPoint) > _navMeshAgent.stoppingDistance+0.5f)
        {
            _navMeshAgent.isStopped = false;
            _navMeshAgent.SetDestination(_destinationPoint);
            _navMeshAgent.speed = _config.ChaseSpeed;
            if(Physics.Raycast(transform.position+Vector3.up,_target.transform.position- transform.position, out var hit, 10))
            {
                if (hit.collider.TryGetComponent<PlayerView>(out var player))
                    _navMeshAgent.velocity = (player.transform.position-transform.position).normalized * _config.ChaseSpeed;
            }
        }
        transform.LookAt(_target.transform);

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
        _animator.SetTrigger("exit combat");
        _animator.SetTrigger("dying");
        _states[NPCState.Attacking] = false;
        _states[NPCState.Chasing] = false;
        _states[NPCState.Patroling] = false;
        _navMeshAgent.velocity = Vector3.zero;
        _navMeshAgent.isStopped = true;
        _animator.SetLayerWeight(_animator.GetLayerIndex("CombatLayer"), 0);
        GetComponent<Collider>().enabled = false;
    }

    public void TakeDamage(int damage, float pushbackForce)
    {
        if (_health <= 0) return;
        _health -= damage;
        if (_health <= 0) Die();
    }
}
