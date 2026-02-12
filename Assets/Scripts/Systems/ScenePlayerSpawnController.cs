using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class ScenePlayerSpawnController : NetworkBehaviour
{
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private GameObject playerPrefab;
    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        StartCoroutine(SpawnNextFrame());
    }
    private IEnumerator SpawnNextFrame()
    {
        yield return null; // wait 1 frame (Start done)

        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            SpawnPlayerForClient(client.ClientId);
        }
    }
    private void SpawnPlayerForClient(ulong clientId)
    {
        if (NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject != null)
            return;
        
        var spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];

        var player = Instantiate(
            playerPrefab,
            spawnPoint.position,
            spawnPoint.rotation
        );
        // player.setUp();
        player.GetComponent<NetworkObject>()
              .SpawnAsPlayerObject(clientId, true);
    }
}
