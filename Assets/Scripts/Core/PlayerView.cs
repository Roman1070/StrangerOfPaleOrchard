using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Zenject;

public class PlayerView : MonoBehaviour
{
    public CharacterController Controller;
    public Transform Model;
    public Transform GroundChecker;
    public Transform SpineAnchor;
    public Transform HandAnchor;
    public Transform WeaponsHolder;
    public CharacterControllerMoveAnimation MoveAnim;

    private void Start()
    {
        transform.position = new Vector3(25.3f, 5, 68.3f);
        GetComponent<NavMeshAgent>().enabled = true;
    }
}