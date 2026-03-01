using UnityEngine;
using Unity.Netcode;
using System;

public class PlayerStats : NetworkBehaviour
{
    [Header("Stats (Network Synced)")]
    // public NetworkVariable<String> playerName =
    //     new NetworkVariable<String>("Playername",
    //         NetworkVariableReadPermission.Everyone,
    //         NetworkVariableWritePermission.Owner);
    public NetworkVariable<ClassType> playerClass =
        new NetworkVariable<ClassType>(ClassType.Human,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);
    public NetworkVariable<int> level =
        new NetworkVariable<int>(1,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);

    public NetworkVariable<float> currentHP =
        new NetworkVariable<float>(100,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);

    //bonus stats from player upgrades
    public NetworkVariable<BonusStats> bonusStats =
        new NetworkVariable<BonusStats>(new BonusStats(),
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);
    
    // completed stats base + bonus stats without anyactive buffs
    public NetworkVariable<Stats> stableStats =
    new NetworkVariable<Stats>(new Stats(),
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);
    // modifiable stats when playing
    public NetworkVariable<Stats> stats =
        new NetworkVariable<Stats>(new Stats(),
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);
    [SerializeField] GameObject uiPrefab;
    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            LoadFromLobby();
            RecalculateStableStats();
            SetStartStat();
        }
        if (IsOwner)
        {
            currentHP.OnValueChanged += OnHPChanged;
            UIManager.Instance.ActivePlayerHUD(gameObject);
        }
    }
    
    private void LoadFromLobby()
    {
        if (!IsServer) return;

        if (LobbyNetwork.Instance == null)
        {
            Debug.LogWarning("LobbyNetwork not found");
            return;
        }

        ulong clientId = OwnerClientId;

        PlayerSaveData save =
            LobbyNetwork.Instance.GetSaveData(clientId);

        // Apply save data
        level.Value = save.level;
        playerClass.Value = save.characterClass;

        bonusStats.Value = save.bonusStats;

        Debug.Log($"Loaded save for {clientId}");
    }

    public void TakeDamage(float amount)
    {
        if (!IsServer) return;
        float def = stats.Value.defense;

        // Get reduction in range 0 - 1
        float reduction = GetDamageReduction(def);

        float finalDamage = amount * (1f - reduction);

        currentHP.Value =
            Mathf.Max(currentHP.Value - finalDamage, 0f);

        Debug.Log(
            $"DMG:{amount} DEF:{def} RED:{reduction * 100f:F1}% FINAL:{finalDamage}"
        );

        if (currentHP.Value <= 0)
            OnDeath();
    }
    public static float GetDamageReduction(float defense)
    {
        const float p = 2.5f;
        const float k = 90f;

        // Prevent negative defense
        defense = Mathf.Max(0f, defense);

        float defPow = Mathf.Pow(defense, p);
        float kPow = Mathf.Pow(k, p);

        return defPow / (defPow + kPow);
    }

    public void Heal(float amount)
    {
        if (!IsOwner) return;
        currentHP.Value = Mathf.Min(currentHP.Value + amount, stats.Value.health);
    }

    private void OnHPChanged(float oldValue, float newValue)
    {
        Debug.Log($"{OwnerClientId} HP: {oldValue} -> {newValue}");
    }

    private void OnDeath()
    {
        Debug.Log($"{OwnerClientId} has died");
        // You can handle respawn or death logic here
    }
    public ClassType GetClassType()
    {
        return playerClass.Value;
    }
    public void RecalculateStableStats()
    {
        ClassStatData data = GameDatabase.Instance.GetClassDatabase()
            .GetClassStatData(playerClass.Value);

        if (data == null)
        {
            Debug.LogError("No ClassStatData for " + playerClass.Value);
            return;
        }
        int lvl = level.Value;

        Stats calStats;
        calStats.health =
            data.GetHealth(lvl) + bonusStats.Value.bonusHealth;
        calStats.defense =
            data.GetDefense(lvl) + bonusStats.Value.bonusDefense;
        calStats.critRate =
            data.GetCritRate(lvl) + bonusStats.Value.bonusCritChance;
        calStats.critDamage =
            data.GetCritDamage(lvl) + bonusStats.Value.bonusCritDamage;
        calStats.extraDamage = bonusStats.Value.bonusDamage;

        stableStats.Value = calStats;   
        
        //display server stats for debugging
        print($"Base Stat : {data.GetHealth(lvl)}, {data.GetDefense(lvl)}, {data.GetCritRate(lvl)}, {data.GetCritDamage(lvl)}");
        print($"Bonus Stat: {bonusStats.Value}");
        print($"Active Stat Level {lvl}: {stableStats.Value}");
    }
    private void SetStartStat()
    {
        Stats activeStat = stableStats.Value;
        stats.Value = activeStat;
        currentHP.Value = stats.Value.health;
    }
}
