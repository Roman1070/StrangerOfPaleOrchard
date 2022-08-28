using DG.Tweening;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using Zenject;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using System.Linq;

public class EnemyNPCView : NPCViewBase, IDamagable, IPlayerTarget, IPunObservable
{
    [Inject] private UpdateProvider _updateProvider;

    protected override NPCType NPCType => NPCType.Enemy;

    public Transform Target => transform;
    public int Id => 1;

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

    private Vector3 _startPoint;
    private NPCAttackData _currentAttack;
    private Vector3 _destinationPoint;
    private IDamagable _target;
    private Coroutine _attackRoutine;
    [SerializeField]
    private float health;

    private float DistanceToTarget => Vector3.Distance(transform.position, _target.Transform.position);

    [property: SerializeField]
    public float Health { get => health; private set => health = value; }

    public Transform Transform => transform;

    public bool IsAlive => Health > 0;


    private void Start()
    {
        Health = _config.MaxHealth;
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

    private void UpdateState()
    {
        var collidersInDetectionRange = Physics.OverlapSphere(transform.position, _config.DetectionDistance, LayerMask.GetMask("Player"));
        if (collidersInDetectionRange.Length > 0)
        {
            if (_target == null)
            {
                _target = collidersInDetectionRange[0].transform.GetComponent<IDamagable>();
            }
        }
        else
        {
            _target = null;
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
        _target.TakeDamage(10);

        while (_target != null && DistanceToTarget <= _config.AttackRange)
        {
            if (Health <= 0) yield return null;
            yield return new WaitForSeconds(_currentAttack.Duration);
            _currentAttack = _config.GetRandomAttack(_currentAttack.Id);
            _animator.SetTrigger(_currentAttack.Id);
            _target.TakeDamage(10);
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

    [PunRPC]
    private void RemoteDie()
    {
        Die();
    }

    private void Die()
    {
        _animator.SetTrigger("exit combat");
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
    }

    public void TakeDamage(int damage)
    {
        Health -= damage;

        if (Health <= 0)
        {
            Die();
           // _photonView.RPC("RemoteDie", RpcTarget.Others);
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(Health);
            stream.SendNext(_target!=null? _target.Transform.GetComponent<PhotonView>().ViewID :-1);
        }
        else
        {
            Health = (float)stream.ReceiveNext();
            if (Health <= 0)
            {
                if(!_states[NPCState.Dying])Die();
            }
            int viewId = (int)stream.ReceiveNext();
            if(viewId!=-1) _target = PhotonView.Find(viewId).GetComponent<IDamagable>();
        }
    }
}
