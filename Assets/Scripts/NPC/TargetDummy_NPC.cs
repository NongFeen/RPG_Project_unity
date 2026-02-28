using Unity.Netcode;
using UnityEngine;
using System.Collections;
using Unity.Services.Matchmaker.Models;
using Pathfinding;
using System;

public class TargetDummy_NPC : BaseNPC
{
    Vector3 spawnPos;
    public override void OnNetworkSpawn()
    {
        spawnPos = this.transform.position;
        base.OnNetworkSpawn();
    }
    protected override void Update()
    {
        this.currentHealth.Value = maxHealth;
        this.transform.position = spawnPos;
    }
    protected override void Attack()
    {
        throw new NotImplementedException();
    }
}