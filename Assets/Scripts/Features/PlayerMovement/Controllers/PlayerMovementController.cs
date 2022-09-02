using Photon.Pun;
using UnityEngine;
using UnityEngine.AI;

public class PlayerMovementController : PlayerMovementControllerBase
{
    private Animator _animator;
    private NavMeshAgent _agent;
    private bool _destinationSet;
    private Transform _target;
    private Vector3 _destination;
    private Camera _camera;
    private PlayerStatesService _states;

    private bool MovementAvailable => new bool[]
    {
        _states.States[PlayerState.BrowsingUI],
        _states.States[PlayerState.Interacting]
    }.Sum().Inverse();

    public PlayerMovementController(SignalBus signalBus, PlayerView player, UpdateProvider updateProvider, Camera camera, PlayerStatesService states) : base(signalBus, player, updateProvider,states)
    {
        _camera = camera;
        _states = states;
        _animator = player.Model.GetComponent<Animator>();
        _agent = player.GetComponent<NavMeshAgent>();
        _updateProvider.Updates.Add(Update);
    }
    private void Update()
    {
        if (!_player.Photon.IsMine) return;

        CheckInput();
        Move();
    }

    private void Move()
    {
        if (_target != null)
        {
            _agent.SetDestination(_target.position);
        }
        else if (_destinationSet)
        {
            if (_agent.destination != _destination)
            {
                _agent.SetDestination(_destination);
                Vector3 direction = (_destination - _player.transform.position).normalized;
                Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
                _player.transform.rotation = Quaternion.Slerp(_player.transform.rotation, lookRotation, Time.deltaTime * 5);
            }
        }
        if (_agent.velocity.magnitude >= 0.8f)
        {
            _animator.SetFloat("Speed", 1, 0.15f, Time.deltaTime);
            _player.Photon.RPC("SendRemoteMovementSpeed", RpcTarget.Others, 1);
        }
        else
        {
            _animator.SetFloat("Speed", 0, 0.15f, Time.deltaTime);
            _player.Photon.RPC("SendRemoteMovementSpeed", RpcTarget.Others, 0);
        }
    }

    private void CheckInput()
    {
        if (Input.GetMouseButton(0) && MovementAvailable)
        {
            if (Physics.Raycast(_camera.ScreenPointToRay(Input.mousePosition), out var hit))
            {
                if (Input.mousePosition.x > Screen.width * 0.85f || Input.mousePosition.x < Screen.width * 0.1f
                    || Input.mousePosition.y < Screen.height * 0.15f || Input.mousePosition.y > Screen.width * 0.9f) return;
                if (hit.collider.TryGetComponent(out IPlayerTarget target))
                {
                    _target = target.Target;
                    _destinationSet = false;
                }
                else
                {
                    _destination = hit.point;
                    _destinationSet = true;
                    _target = null;
                }
            }
        }
    }
}
