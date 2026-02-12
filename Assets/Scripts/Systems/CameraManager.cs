using UnityEngine;
using Unity.Netcode;

public class CameraManager : NetworkBehaviour
{
    public GameObject cameraHolder;
    public Vector3 cameraOffset = new Vector3(0, 0, -10);

    private bool isCameraActive = false;
    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            // Activate only this player's camera
            cameraHolder.SetActive(true);
            isCameraActive = true;
        }
        else
        {
            // Disable other players' cameras
            cameraHolder.SetActive(false);
        }
    }
    void Update()
    {
        if (!isCameraActive) return;
        
        cameraHolder.transform.position = transform.position + cameraOffset;
    }
}
