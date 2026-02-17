using System.Collections.Generic;
using UnityEngine;

public class ExitTileTrigger : TileTrigger
{

    public override void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;
        if(isOnceTimeTrigger)
        {
            foreach (var tile in linkedTile)
                Destroy(tile);
        }
        MapManager.Instance.PlayerTriggeredExtractTile();
    }
}

