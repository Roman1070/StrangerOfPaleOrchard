using System.Collections.Generic;
using UnityEngine;

public class OtherPlayersContainer : MonoBehaviour
{
    public List<PlayerView> Players;

    private void Awake()
    {
        Players = new List<PlayerView>();
    }

    public void Insert(PlayerView player) => Players.Add(player);
    public void Remove(PlayerView player) => Players.Remove(player);
}