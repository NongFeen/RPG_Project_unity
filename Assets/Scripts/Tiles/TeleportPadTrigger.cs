using System.Collections.Generic;
using UnityEngine;

public class TeleprotPadTrigger : MonoBehaviour
{
    [SerializeField] Transform teleportTarget;
    [SerializeField] private TeleportGroup group;
    public void OnTriggerStay2D(Collider2D collider)
    {
        if (!collider.CompareTag("Player")) return;

        if (collider.TryGetComponent<Player>(out var player))
        {
            RaidMapManager_TeleportRoom raidMap =
                MapManager.Instance as RaidMapManager_TeleportRoom;

            if (raidMap != null)
            {
                raidMap.RequestTeleportServerRpc(
                    group,
                    player.NetworkObjectId,
                    teleportTarget.position
                );
            }
        }
    }
}

