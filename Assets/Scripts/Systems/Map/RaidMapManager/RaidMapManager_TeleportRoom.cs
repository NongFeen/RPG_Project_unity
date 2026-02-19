using System.Collections.Generic;
using Pathfinding;
using Unity.Netcode;
using UnityEngine;

public class RaidMapManager_TeleportRoom : MapManager
{
    [SerializeField]private NetworkVariable<float> pad1 = new();
    [SerializeField]private NetworkVariable<float> pad2 = new();
    [SerializeField]private NetworkVariable<float> pad3 = new();
    [SerializeField]private NetworkVariable<float> pad4 = new();
    [SerializeField]private float TeleporterCooldown = 5;
    [SerializeField]private float TeleporterTimer = 0;

    // 0,1 for start room
    // 2,3 for crossed room
    void Start()
    {
        mapItemDrop = GameDatabase.Instance.GetMapDatabase().GetMapData(GameManager.Instance.selectMapName).mapItemDrop;
        AstarPath.active.Scan();
    }
    public override void OnNetworkSpawn()
    {
        if(!IsServer) return;
        pad1.Value = 100f;
        pad2.Value = 100f;
        pad3.Value = 100f;
        pad4.Value = 100f;
        base.OnNetworkSpawn();
    }
    void Update()
    {
        if(!IsServer) return;
        TeleporterTimer -= Time.deltaTime;
    }

    bool IsTeleportReady(TeleportGroup group)
    {
        if(TeleporterTimer > 0) return false;
        switch (group)
        {
            case TeleportGroup.StartRoom:
                return pad1.Value >= 100 && pad2.Value >= 100;

            case TeleportGroup.CrossRoom:
                return pad3.Value >= 100 && pad4.Value >= 100;
        }

        return false;
    }
    [ServerRpc(RequireOwnership = false)]
    public void RequestTeleportServerRpc(TeleportGroup group,ulong playerId,Vector3 targetPos)
    {
        if (!IsTeleportReady(group))
            return;

        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(playerId, out var netObj))
        {
            // netObj.transform.position = targetPos;
            netObj.TryGetComponent<PlayerMovement>(out var playerMovement);
            playerMovement.TeleportClientRpc(targetPos);
            // TeleportClientRpc(targetPos);
            OnUseTeleportCharge(group);
        }
    }
    void OnUseTeleportCharge(TeleportGroup group)
    {
        switch (group)
        {
            case TeleportGroup.StartRoom:
                pad1.Value = 0;
                pad2.Value = 0;
                break;

            case TeleportGroup.CrossRoom:
                pad3.Value = 0;
                pad4.Value = 0;
                break;
        }
        TeleporterTimer = TeleporterCooldown;
    }
    [ClientRpc]
    void TeleportClientRpc(Vector3 pos)
    {
        print("teleport client");
        // player.transform.position = pos;
    }
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