using Photon.Pun;
using UnityEngine;
using UnityEngine.AI;
using DG.Tweening;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField]
    private PlayerView _player;
    [SerializeField]
    private NavMeshAgent _agent;
    [SerializeField]
    private PhotonView _photon;
    [SerializeField]
    private Animator _animator;
    [SerializeField]
    private float _attackRange;

    private bool _destinationSet;
    private Transform _target;
    private Vector3 _destination;
    private Camera _camera;

    private void Start()
    {
        _camera = FindObjectOfType<MainCameraAnchor>().Camera;
    }

    private void Update()
    {
        if (!_photon.IsMine) return;

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
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime*5);
            }
        }
        if (_agent.velocity.magnitude >= 0.8f)
        {
            _animator.SetFloat("Speed", 1, 0.15f, Time.deltaTime);
        }
        else
        {
            _animator.SetFloat("Speed", 0, 0.15f, Time.deltaTime);
        }
    }

    private void CheckInput()
    {
        if (Input.GetMouseButtonDown(0))
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
