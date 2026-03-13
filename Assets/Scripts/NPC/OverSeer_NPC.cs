using Unity.Netcode;
using UnityEngine;
using Pathfinding;

public class OverSeer_NPC : BaseNPC
{
    [Header("References")]
    [SerializeField] public NetworkVariable<OverseerState> currentState = new NetworkVariable<OverseerState>();
    [SerializeField] private GameObject bulletPrefab;
    private AIPath aiPath;
    public enum OverseerState
    {
        Idle,
        Shooting,
        Move
    }


    [Header("Idle Movement")]
    [SerializeField] private float idleMoveMinTime = 2f;
    [SerializeField] private float idleMoveMaxTime = 4f;
    [SerializeField] private float idleMoveDistance = 4f;
    [SerializeField] private float idleSpeed = 4f;
    private float nextIdleMoveTime;
    private Vector3 idleDestination;
    [Header("Shooting")]
    [SerializeField] private float aimTime = 2f;

    [SerializeField] private float losConfirmTime = 0.25f;
    private float losTimer = 0f;

    [SerializeField]private float stateTimer;
    

    protected override void Awake()
    {
        aiPath = GetComponent<AIPath>();
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
            currentState.Value = OverseerState.Idle;
    }

    protected override void Update()
    {
        if (!IsServer) return;
        switch (currentState.Value)
        {
            case OverseerState.Idle:
                HandleIdle();
                break;

            case OverseerState.Shooting:
                HandleShooting();
                break;

            case OverseerState.Move:
                HandleMove();
                break;
        }
    }
    void SetState(OverseerState newState)
    {
        currentState.Value = newState;
        switch (newState)
        {
            case OverseerState.Idle :
                stateTimer = 0f;
                 break;
            case OverseerState.Shooting:
                stateTimer = 0f;
                break;
            case OverseerState.Move: 
                aiPath.canMove = true;
                aiPath.maxSpeed = idleSpeed;
                stateTimer = 0f;
                break;
        }
    }

    void HandleIdle()
    {
        stateTimer += Time.deltaTime;
        FindTarget();
        //switch state if found target
        if (target != null)
        {
            currentState.Value = OverseerState.Shooting;
            return;
        }
        aiPath.maxSpeed = idleSpeed;
        aiPath.canMove = true;
        // idle move time
        if (stateTimer >= nextIdleMoveTime)
        {
            Vector2 dir = GetRandomCompassDirection();

            idleDestination = transform.position + (Vector3)(dir * idleMoveDistance);

            aiPath.destination = idleDestination;

            nextIdleMoveTime = Random.Range(idleMoveMinTime, idleMoveMaxTime);
            stateTimer = 0f;
        }
    }
    void HandleShooting()
    {
        //stop to aim then shoot after short time
        stateTimer += Time.deltaTime;
        aiPath.canMove = false;
        if(stateTimer >= aimTime )//shoot even player goin out of los
        {
            SpawnProjectile(target.transform);
            SetState(OverseerState.Move);
        }
    }

    void HandleMove()
    {
        stateTimer += Time.deltaTime;

        FindTarget();

        if (target == null)
        {
            SetState(OverseerState.Idle);
            return;
        }

        

        if (!HasLineOfSight(target))
        {
            losTimer = 0f; // reset buffer
            SetDestination(target.position);
            return;
        }

        // LOS detected → wait a bit before stopping
        losTimer += Time.deltaTime;

        float t = Mathf.Clamp01(losTimer / losConfirmTime);
        float smooth = 1f - Mathf.Pow(t, 2f); // ease-out curve
        aiPath.maxSpeed = idleSpeed * smooth;

        if (losTimer >= losConfirmTime)
        {
            aiPath.canMove = false;
            SetState(OverseerState.Shooting);
        }
    }

    protected override void FindTarget()
    {
        var players = GameObject.FindGameObjectsWithTag("Player");
        Transform bestCandidate = null;
        float closestDistance = detectionRange;
        foreach (var p in players)
        {
            if (!p.TryGetComponent<PlayerStats>(out var stats))
                continue;

            if (stats.currentHP.Value <= 0)
                continue;

            float dist = Vector2.Distance(transform.position, p.transform.position);

            if (dist > detectionRange)
                continue;

            if (!HasLineOfSight(p.transform))
                continue;

            if (dist < closestDistance)
            {
                closestDistance = dist;
                bestCandidate = p.transform;
            }
        }

        if (target == null)
        {
            target = bestCandidate;
            return;
        }

        if (target.TryGetComponent<PlayerStats>(out var currentStats))
        {
            if (currentStats.currentHP.Value <= 0)
            {
                target = bestCandidate;
                return;
            }
        }

        if (bestCandidate != null)
        {
            float currentDist = Vector2.Distance(transform.position, target.position);

            if (closestDistance < currentDist)
            {
                target = bestCandidate;
            }
        }
    }
    Vector2 GetRandomCompassDirection()
    {
        Vector2[] dirs = new Vector2[]
        {
            new Vector2(1,1),
            new Vector2(-1,1),
            new Vector2(-1,-1),
            new Vector2(1,-1),
            new Vector2(1,0),
            new Vector2(-1,0),
            new Vector2(0,-1),
            new Vector2(0,1)
        };

        return dirs[Random.Range(0, dirs.Length)].normalized;
    }

    bool HasLineOfSightFromPoint(Vector2 from, Vector2 to)
    {
        Vector2 direction = (to - from).normalized;
        float distance = Vector2.Distance(from, to);

        RaycastHit2D hit = Physics2D.Raycast(
            from,
            direction,
            distance,
            obstacleLayer
        );

        return hit.collider == null;
    }
    void SpawnProjectile(Transform targetTransform)
    {
        if (!IsServer) return;
        if (bulletPrefab == null) return;

        if (!bulletPrefab.TryGetComponent<NetworkObject>(out var netObjToUse))
            return;
        Vector2 dir = (targetTransform.position - transform.position).normalized;
        Vector3 magicProjectileOffsetValue = new Vector3(0, 0,0);
        Vector3 spawnPos = transform.position + magicProjectileOffsetValue;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        Quaternion rot = Quaternion.Euler(0, 0, angle);
        // Quaternion rot = Quaternion.identity;

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
                dir, 
                contactDamage,        
                false,                
                1f                    
            );
        }
    }
    

    void SetDestination(Vector2 pos)
    {
        if (aiPath != null)
        {
            aiPath.destination = pos;
            aiPath.SearchPath();
        }
    }

    protected override void Attack()
    {
       //not contact damage
    }
}