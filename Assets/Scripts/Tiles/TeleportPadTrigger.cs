using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class TeleprotPadTrigger : MonoBehaviour
{
    [SerializeField] Transform teleportTarget;
    [SerializeField] private TeleportGroup group;
    [SerializeField] private Light2D padLight;
    [SerializeField] private float minimumLightIntensity = 0.5f;
    [SerializeField] private float maximumLightIntensity = 1.5f;
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

