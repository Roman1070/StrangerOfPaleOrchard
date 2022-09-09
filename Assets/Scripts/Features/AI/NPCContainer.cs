using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCContainer : MonoBehaviour
{
    public NPCViewBase[] NPCs { get; private set; }

    private void Awake()
    {
        NPCs = FindObjectsOfType<NPCViewBase>();
    }
}
