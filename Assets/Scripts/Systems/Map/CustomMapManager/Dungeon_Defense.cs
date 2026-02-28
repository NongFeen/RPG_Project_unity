using System;
using System.Collections.Generic;
using Pathfinding;
using Unity.Netcode;
using UnityEngine;

public class Dungeon_Defense : MapManager
{
    [Header("Survive Setting")]
    [SerializeField] public float mapTimer = 0f;
    [SerializeField] public NetworkVariable<int> currentWave = new NetworkVariable<int>(0);
    [SerializeField] public ChargingPadTrigger chargingPad;

    void Start()
    {
        mapItemDrop = GameDatabase.Instance.GetMapDatabase().GetMapData(GameManager.Instance.selectMapName).mapItemDrop;
        AstarPath.active.Scan();
    }
    public override void OnNetworkSpawn()
    {
        currentWave.OnValueChanged += OnChangeCurrentWave;
        if (IsServer)
        {
            chargingPad.OnPadFullyCharged += OnPadFullyCharge;
        }
        base.OnNetworkSpawn();
    }
    public override void OnNetworkDespawn()
    {
        if (IsServer)
        {
            chargingPad.OnPadFullyCharged -= OnPadFullyCharge;
        }
        base.OnNetworkDespawn();
    }
    void Update()
    {
        if(!IsServer) return;
        mapTimer+= Time.deltaTime;
    }
    private void OnChangeCurrentWave(int previousValue, int newValue)
    {
        //client show new wave is coming
    }

    private void OnPadFullyCharge()
    {
        if (!IsServer) return;
        StartNextWave();
        chargingPad.ResetTime();
    }
    private void StartNextWave()
    {
        if (!IsServer) return;
        int nextWave = currentWave.Value + 1;

        if (nextWave > 5)
        {
            Debug.Log("All wave is have been send");
            CheckMapCompletion();
            return;
        }

        Debug.Log("Starting Wave " + nextWave);

        int baseID = (nextWave-1) * 10;   // 10,20,30,40,50 pattern

        for (int i = 1; i <= 8; i++)
        {
            string spawnID = (baseID + i).ToString();
            SpawnNPCManager.Instance.SpawnAtPoint(spawnID, true);
        }
            currentWave.Value = nextWave;
    }

    public override void CheckMapCompletion()
    {
        if (!IsServer) return;

        if (aliveEnemyCount.Value <= 0 && currentWave.Value >=5)
        {
            CompleteMap();
        }
    }
    public override void CompleteMap()
    {
        print("COMPLETE");
        base.CompleteMap();
    }
    public override void OnBossDefeated()
    {
        //do nothingeven boss died
    }
}

