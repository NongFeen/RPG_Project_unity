using Unity.Netcode;
using UnityEngine;
using System.Collections;
using Unity.Services.Matchmaker.Models;
using Pathfinding;
using System;
public class Skeleton_NPC : BaseNPC
{
    AIPath aiPath;

    protected override void Awake()
    {
        base.Awake();
        aiPath = GetComponent<AIPath>();
        aiPath.maxSpeed = moveSpeed;
    }

    protected override void Attack()
    {
        if (target == null) return;

        if (target.TryGetComponent<PlayerStats>(out var player))
        {
            player.TakeDamage(contactDamage);
        }
    }
    protected override void HandleBehavior()
    {
        if(target == null)
        {
            aiPath.canMove = false;
            return;
        }
        float dist = Vector2.Distance(transform.position, target.position);

        if (dist > attackRange)
        {
            aiPath.canMove = true;
            aiPath.destination = target.position;
        }
        else
        {
            aiPath.canMove = false;
            TryAttack(); // uses BaseNPC cooldown system
        }
    }
}
