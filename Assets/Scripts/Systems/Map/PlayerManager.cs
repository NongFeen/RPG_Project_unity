using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance;

    public List<PlayerStats> Players = new();

    public event Action<PlayerStats> OnPlayerRegistered;
    public event Action<PlayerStats> OnPlayerUnregistered;

    private void Awake()
    {
        Instance = this;
    }

    public void RegisterPlayer(PlayerStats player)
    {
        if (!Players.Contains(player))
        {
            Players.Add(player);
            OnPlayerRegistered?.Invoke(player);
        }
    }

    public void UnregisterPlayer(PlayerStats player)
    {
        if (Players.Remove(player))
        {
            OnPlayerUnregistered?.Invoke(player);
        }
    }
}