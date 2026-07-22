using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class TeleprotPadTrigger : MonoBehaviour
{
    [SerializeField] Transform teleportTarget;
    [SerializeField] private TeleportGroup group;
    [SerializeField] private Light2D padLight;
    [SerializeField] private float idleLightIntensity = 0.15f;
    [SerializeField] private float minimumBlinkIntensity = 0.3f;
    [SerializeField] private float maximumBlinkIntensity = 1.2f;
    [SerializeField] private float blinkSpeed = 0.75f;

    private void Awake()
    {
        if (padLight != null)
        {
            padLight.enabled = true;
            padLight.intensity = idleLightIntensity;
        }
    }

    private void Update()
    {
        if (padLight == null) return;

        RaidMapManager_TeleportRoom raidMap =
            MapManager.Instance as RaidMapManager_TeleportRoom;
        bool isReady = raidMap != null && raidMap.IsTeleportReady(group);

        if (isReady)
        {
            float blink = (Mathf.Sin(Time.time * blinkSpeed * Mathf.PI * 2f) + 1f) * 0.5f;
            padLight.intensity = Mathf.Lerp(minimumBlinkIntensity, maximumBlinkIntensity, blink);
        }
        else
        {
            padLight.intensity = idleLightIntensity;
        }
    }

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
