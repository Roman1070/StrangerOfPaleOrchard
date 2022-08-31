using Photon.Pun;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField]
    private PlayerView _player;
    [SerializeField]
    private NavMeshAgent _agent;
    [SerializeField]
    private Animator _animator;
    [SerializeField]
    private float _attackRange;

    private UpdateProvider _updateProvider;
    private bool _destinationSet;
    private Transform _target;
    private Vector3 _destination;
    private Camera _camera;

    private void Start()
    {
        _camera = FindObjectOfType<MainCameraAnchor>().Camera; //не могу заинжектить потому что нужен спавн плэера через фотон
        _updateProvider = FindObjectOfType<UpdateProvider>();
        _updateProvider.Updates.Add(LocalUpdate);
    }

    private void LocalUpdate()
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
                Vector3 direction = (_destination - transform.position).normalized;
                Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5);
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
        if (Input.GetMouseButton(0))
        {
            if (Physics.Raycast(_camera.ScreenPointToRay(Input.mousePosition), out var hit))
            {
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
