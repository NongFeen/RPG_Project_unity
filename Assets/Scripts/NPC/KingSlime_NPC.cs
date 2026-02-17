using Unity.Netcode;
using UnityEngine;
using System.Collections;
using Unity.Services.Matchmaker.Models;
using Pathfinding;
using System;

public class KingSlime_NPC : BaseNPC
{
    [Header("Boss Attack")]
    [SerializeField] GameObject aoeProjectilePrefab;
    [Header("Jump Settings")]
    [SerializeField] float jumpUpDuration = 0.3f;
    [SerializeField] float moveMinDuration = 0.2f;
    [SerializeField] float slamDuration = 1f;
    [SerializeField] float attackCooldownTime = 2f;
    [SerializeField] LayerMask NPCLayerMask;
    [SerializeField] int NPCLayer;
    [SerializeField] LayerMask ignoreProjectileLayerMask;
    [SerializeField] int ignoreProjectileLayer;
    [SerializeField] float magicProjectileOffset;
    Vector3 jumpTargetPosition;
    float stateTimer;
    bool inCombat =false;
    AIPath aiPath;
    AIDestinationSetter destSetter;
    #region State
    public NetworkVariable<KingSlimeState> npcState = new(KingSlimeState.Idle,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);
    public enum KingSlimeState{Idle,SelectTarget,JumpUp,JumpMove,Slam,Recover}
    #endregion
    protected override void Awake()
    {
        aiPath = GetComponent<AIPath>();
        destSetter = GetComponent<AIDestinationSetter>();
        NPCLayer = (int)Mathf.Log(NPCLayerMask.value, 2);
        ignoreProjectileLayer = (int)Mathf.Log(ignoreProjectileLayerMask.value, 2);

        // // print(ignoreProjectileLayerMask.value);
        // print((int)Math.Log(ignoreProjectileLayerMask.value,2));
        // print(NPCLayerMask.value);
        // print((int)Math.Log(ignoreProjectileLayerMask.value,2));

        base.Awake();
    }
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        npcState.OnValueChanged += OnStateChanged;
        OnStateChanged(npcState.Value, npcState.Value);
    }
    public override void OnNetworkDespawn()
    {
        npcState.OnValueChanged -= OnStateChanged;
        base.OnNetworkDespawn();
    }
    protected override void Attack()
    {
        throw new System.NotImplementedException();
    }
    protected override void Update()
    {
        if (!IsServer || IsDead) return;
        stateTimer += Time.deltaTime;
        switch (npcState.Value)
        {
            case KingSlimeState.Idle:
                UpdateIdle();
                break;

            case KingSlimeState.SelectTarget:
                SelectRandomPlayer();
                break;

            case KingSlimeState.JumpUp:
                JumpUp();
                break;
            case KingSlimeState.JumpMove:
                JumpMove();
                break;
            case KingSlimeState.Slam:
                // Slam();
                //just wait for animation
                break;

            case KingSlimeState.Recover:
                Recovering();
                break;
        }
    }

    void SetState(KingSlimeState newState)
    {
        npcState.Value = newState;
        switch(newState){
            case KingSlimeState.Idle:
                animator.SetBool("JumpToAir", false);
                aiPath.canMove = false;
                destSetter.target = null;
                break;
            case KingSlimeState.JumpUp:
                stateTimer = 0;
                aiPath.canMove = false;
                animator.SetBool("JumpToAir", true);
                break;
            case KingSlimeState.JumpMove:
                stateTimer = 0;
                aiPath.canMove = true;
                destSetter.target = null;
                jumpTargetPosition = target.position;
                break;
            case KingSlimeState.Slam:
                stateTimer = 0;
                aiPath.canMove = false;
                destSetter.target = null;
                animator.SetBool("JumpToAir", false);
                animator.SetBool("Slam", true);
                break;
            case KingSlimeState.Recover:
                stateTimer = 0;
                animator.SetBool("Slam", false);
                animator.SetBool("JumpToAir", false);
                aiPath.canMove = false;
                destSetter.target = null;
                break;
            default:
                stateTimer = 0;
                break;
        }
    }
    void UpdateIdle()
    {
        var players = GameObject.FindGameObjectsWithTag("Player");
        if (players.Length > 0)
        {
            SetState(KingSlimeState.SelectTarget);
        }
    }
    void SelectRandomPlayer()
    {
        var players = GameObject.FindGameObjectsWithTag("Player");
        //found no player
        if (players.Length == 0)
        {
            SetState(KingSlimeState.Idle);
            inCombat = false;
            return;
        }

        //randomly select a player
        int index = UnityEngine.Random.Range(0, players.Length);
        target = players[index].transform;
        float currentDetectionRange = inCombat ? Mathf.Infinity : detectionRange;
        if(Vector2.Distance(transform.position,target.position) < currentDetectionRange )// player not in range and attack in cooldown
        {
            destSetter.target = null;
            aiPath.canMove = false;
            inCombat = true;
            SetState(KingSlimeState.JumpUp);
        }
    }
    void JumpUp()
    {
        //Jump to the air 
        //set animation to JumpToAir
        if(stateTimer >= jumpUpDuration)
        {
            //finish jump up
            SetState(KingSlimeState.JumpMove);
        }
    }
    void JumpMove()
    {
        if(target == null)
        {
            SetState(KingSlimeState.Idle);
            return;
        }
        //move toward target pos
        // aiPath.destination = jumpTargetPosition;
        destSetter.target = target;
        float dist = Vector2.Distance(transform.position, target.position);
        if (dist <= attackRange && stateTimer > moveMinDuration)
        {
            SetState(KingSlimeState.Slam);
        }
    }
    // void Slam()
    // {
        // if(stateTimer >= slamDuration)
        // {
        //     OnSlamEnd();
        // }
    // }
    public void OnSlamEnd()
    {
        //spawn AOE projectile or make hit box active
        Debug.Log("Slam Ended, spawn AOE");
        SpawnAOEProjectile();
        SetState(KingSlimeState.Recover);
    }
    void Recovering()
    {
        if(stateTimer >= attackCooldownTime)
        {
            SetState(KingSlimeState.Idle);
        }
    }
    void OnStateChanged(KingSlimeState oldState, KingSlimeState newState)//for client
    {
        if(newState == KingSlimeState.JumpUp 
        || newState == KingSlimeState.JumpMove 
        || newState == KingSlimeState.Slam)
        {
            // print(ignoreProjectileLayer);
            gameObject.layer = ignoreProjectileLayer;
        }
        else
        {
            // print(NPCLayer);
            gameObject.layer = NPCLayer;
        }
    }

    void SpawnAOEProjectile()
    {
        if (!IsServer) return;
        if (aoeProjectilePrefab == null) return;

        if (!aoeProjectilePrefab.TryGetComponent<NetworkObject>(out var netObjToUse))
            return;
        
        Vector3 magicProjectileOffsetValue = new Vector3(0, magicProjectileOffset,0);
        Vector3 spawnPos = transform.position + magicProjectileOffsetValue;
        Quaternion rot = Quaternion.identity;

        NetworkObject netObj =
            NetworkManager.Singleton.SpawnManager.InstantiateAndSpawn(
                netObjToUse,
                NetworkManager.Singleton.LocalClientId,
                false, false, false,
                spawnPos,
                rot
            );

        GameObject proj = netObj.gameObject;

        if (proj.TryGetComponent<ServerProjectile>(out var serverProjectile))
        {
            serverProjectile.OnSpawn(
                Vector2.zero, //no dir needed
                contactDamage,        
                false,                
                1f                    
            );
        }
    }
}
