using System;
using System.Collections.Generic;
using UnityEngine;

public class OtherPlayersContainer : MonoBehaviour
{
    public List<PlayerView> Players;
    public event Action<PlayerView> OnPlayerJoined;
    public event Action<PlayerView> OnPlayerRemoved;

    private void Awake()
    {
        Players = new List<PlayerView>();
    }

    public void Insert(PlayerView player)
    {
        Players.Add(player);
        OnPlayerJoined?.Invoke(player);
    }
    public void Remove(PlayerView player) 
    {
        Players.Remove(player);
        OnPlayerRemoved?.Invoke(player);
    }
}