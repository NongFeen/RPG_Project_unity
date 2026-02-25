using Unity.Netcode;
using UnityEngine;

public class FriendShipTrigger : NetworkBehaviour
{
    private NetworkObject currentOccupant;

    public bool IsOccupied => currentOccupant != null;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsServer) return;

        if (!other.CompareTag("Player")) return;

        if (currentOccupant != null) return; // already taken

        NetworkObject netObj = other.GetComponent<NetworkObject>();
        if (netObj == null) return;

        currentOccupant = netObj;

        MapManager.Instance.CheckFriendshipCondition(0);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!IsServer) return;

        if (currentOccupant == null) return;

        NetworkObject netObj = other.GetComponent<NetworkObject>();
        if (netObj == currentOccupant)
        {
            currentOccupant = null;
            MapManager.Instance.CheckFriendshipCondition(0);
        }
    }
}