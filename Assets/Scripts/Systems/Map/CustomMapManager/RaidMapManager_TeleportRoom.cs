using System.Collections.Generic;
using Pathfinding;
using Unity.Netcode;
using UnityEngine;

public class RaidMapManager_TeleportRoom : MapManager
{
    [SerializeField]private ChargingPadTrigger pad1 ;
    [SerializeField]private ChargingPadTrigger pad2 ;
    [SerializeField]private ChargingPadTrigger pad3 ;
    [SerializeField]private ChargingPadTrigger pad4 ;
    [SerializeField]private float TeleporterCooldown = 5;
    [SerializeField]private float TeleporterTimer = 0;
    [SerializeField]private List<FriendShipTrigger> friendShipPlates;
    private readonly NetworkVariable<bool> startRoomTeleportReady = new NetworkVariable<bool>();
    private readonly NetworkVariable<bool> crossRoomTeleportReady = new NetworkVariable<bool>();
    // 1,2 for start room
    // 3,4 for crossed room
    void Start()
    {
        GridGraph grid = AstarPath.active.data.gridGraph;
        grid.center = aStarSetting.center;
        grid.SetDimensions(aStarSetting.width,aStarSetting.depth,aStarSetting.nodeSize);
        AstarPath.active.Scan();
        mapItemDrop = GameDatabase.Instance.GetMapDatabase().GetMapData(GameManager.Instance.selectMapName).mapItemDrop;
        GameManager.Instance.gameState = GameState.InGame;
    }
    public override void OnNetworkSpawn()
    {
        if(!IsServer) return;
        base.OnNetworkSpawn();
    }
    void Update()
    {
        if(!IsServer) return;
        TeleporterTimer = Mathf.Max(0f, TeleporterTimer - Time.deltaTime);
        UpdateTeleportReadiness();
        CheckAllPlayersDeadAndReset();
    }
    #region Teleport
    private void UpdateTeleportReadiness()
    {
        bool cooldownFinished = TeleporterTimer <= 0f;
        startRoomTeleportReady.Value = cooldownFinished && pad1.IsCharged() && pad2.IsCharged();
        crossRoomTeleportReady.Value = cooldownFinished && pad3.IsCharged() && pad4.IsCharged();
    }

    public bool IsTeleportReady(TeleportGroup group)
    {
        return group == TeleportGroup.StartRoom
            ? startRoomTeleportReady.Value
            : crossRoomTeleportReady.Value;
    }

    [ServerRpc(RequireOwnership = false)]
    public void RequestTeleportServerRpc(TeleportGroup group,ulong playerId,Vector3 targetPos)
    {
        if (!IsTeleportReady(group))
            return;

        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(playerId, out var netObj))
        {
            netObj.TryGetComponent<PlayerMovement>(out var playerMovement);
            playerMovement.TeleportClientRpc(targetPos);
            OnUseTeleportCharge(group);
        }
    }
    void OnUseTeleportCharge(TeleportGroup group)
    {
        switch (group)
        {
            case TeleportGroup.StartRoom:
                pad1.ResetTime();
                pad2.ResetTime();
                break;

            case TeleportGroup.CrossRoom:
                pad3.ResetTime();
                pad4.ResetTime();
                break;
        }
        TeleporterTimer = TeleporterCooldown;
        UpdateTeleportReadiness();
    }
    #endregion

    #region FriendShip 
    
    public override void CheckFriendshipCondition(int value)
    {
        if (!IsServer) return;

        int totalPlayers = NetworkManager.Singleton.ConnectedClients.Count;
        int occupied = 0;
        foreach (var plate in friendShipPlates)
        {
            if (plate.IsOccupied)
                occupied++;
        }

        if (occupied >= totalPlayers)
        {
            OnFriendshipComplete();
        }
    }

    private void OnFriendshipComplete()
    {
        Debug.Log("Friendship Complete!");
        CompleteMap();
    }

    #endregion
    public override void CheckMapCompletion()
    {
        // if 4 player stay on exit pad
    }
}

public enum TeleportGroup
{
    StartRoom,
    CrossRoom
}
