using System.Collections.Generic;
using UnityEngine;

public class TileTrigger : MonoBehaviour
{
    [SerializeField]public List<int> spawnerID;
    [SerializeField]public List<GameObject> linkedTile;

    [SerializeField]public bool isOnceTimeTrigger = false;

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;
        print("Tile Triggered by Player: " + spawnerID);
        if(isOnceTimeTrigger)
        {
            foreach (var tile in linkedTile)
                Destroy(tile);
        }
        MapManager.Instance.PlayerTriggeredTile(spawnerID);
    }
}

