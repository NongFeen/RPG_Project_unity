using UnityEngine;
using Unity.Netcode;
using System;
using System.Collections.Generic;

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
    public NetworkVariable<Stats> activeStats =
        new NetworkVariable<Stats>(new Stats(),
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);
    private Dictionary<BuffType, BaseBuff> activeBuffs= new Dictionary<BuffType, BaseBuff>();
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
    public void Update()
    {
        // if (!IsServer) return;
        List<BuffType> toRemove = new();

        foreach (var buff in activeBuffs)
        {
            buff.Value.Update();

            if (buff.Value.IsExpired)
                toRemove.Add(buff.Key);
        }

        foreach (var type in toRemove)
        {
            activeBuffs[type].Remove();
            activeBuffs.Remove(type);
        }

        if (toRemove.Count > 0)
            RecalculateActiveStats();
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
        float def = activeStats.Value.defense;

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
        if (!IsServer) return;
        currentHP.Value = Mathf.Min(currentHP.Value + amount, activeStats.Value.health);
    }
    [ServerRpc]
    public void HealServerRpc(float amount)
    {
        Heal(amount);
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
        calStats.extraDamage = 1.0f +bonusStats.Value.bonusDamage;
        calStats +=bonusStats.Value;
        stableStats.Value = calStats;   
        
        //display server stats for debugging
        print($"Base Stat : {data.GetHealth(lvl)}, {data.GetDefense(lvl)}, {data.GetCritRate(lvl)}, {data.GetCritDamage(lvl)}");
        print($"Bonus Stat: {bonusStats.Value}");
        print($"Active Stat Level {lvl}: {stableStats.Value}");
    }
    private void SetStartStat()
    {
        Stats baseStat = stableStats.Value;
        activeStats.Value = baseStat;
        currentHP.Value = activeStats.Value.health;
    }
    private void RecalculateActiveStats()
    {
        Stats newStats = stableStats.Value;
        newStats += bonusStats.Value;

        foreach (var buff in activeBuffs.Values)
        {
            newStats = buff.ModifyStats(newStats);
        }

        activeStats.Value = newStats;
    }
    public void AddBuffServer(BuffType type, float duration)
    {
        BuffDefinition buff = GameDatabase.Instance.GetBuffDatabase().GetBuffDefinition(type);
        if(buff == null)
        {
            Debug.LogError("BuffDefinition not found for type: " + type);
            return;
        }
        AddBuff(buff, duration);
        AddBuffClientRpc(type, duration);
    }
    public void AddBuff(BuffDefinition data, float duration)
    {
        // if (!IsServer) return;

        if (activeBuffs.ContainsKey(data.buffType))
            return;

        BaseBuff buff = CreateBuffInstance(data, duration);
         if (buff == null)
        {
            Debug.LogError($"Buff type {data.buffType} not implemented!");
            return;
        }
        activeBuffs.Add(data.buffType, buff);

        if(!IsServer) return;
            RecalculateActiveStats();
    }
    [ClientRpc]
    public void AddBuffClientRpc(BuffType type, float duration)
    {
        if(IsServer) return;
        BuffDefinition buff = GameDatabase.Instance.GetBuffDatabase().GetBuffDefinition(type);
        AddBuff(buff, duration);
    }
    private BaseBuff CreateBuffInstance(BuffDefinition def,float duration = 0)
    {
        if (def == null)
        {
            Debug.LogError("BuffDefinition is null");
            return null;
        }
        switch (def.buffType)
        {
            case BuffType.LockedIn:
                return new LockedInBuff(this, def, duration);
            case BuffType.SteelStrong:
                return new SteelStrongBuff(this, def, duration);
            case BuffType.WellOfBlessing:
                return new WellofBlessing(this, def, duration);
            case BuffType.ABigGuy:
                return new ABigGuy(this, def, duration);
            case BuffType.Along:
                return new Along(this, def, duration);
            case BuffType.Arise:
                return new Arise(this, def, duration);
            case BuffType.ChadAura:
                return new ChadAuraBuff(this, def, duration);
            // default:
                // return new BaseBuff(this, def);
                default:
                    return null;
        }
    }
    public Dictionary<BuffType, BaseBuff> GetActiveBuffs()
    {
        return activeBuffs;
    }

 
    public void RemoveBuffServer(BuffType type)
    {
        if (!IsServer) return;
        RemoveBuff(type);
        RemoveBuffClientRpc(type);
    }
    public void RemoveBuff(BuffType type)
    {
        if (!activeBuffs.ContainsKey(type))
            return;

        activeBuffs[type].Remove();
        activeBuffs.Remove(type);
        RecalculateActiveStats();
    }
    [ClientRpc]
    public void RemoveBuffClientRpc(BuffType type)
    {
        if(IsServer) return;
        if (activeBuffs.ContainsKey(type))
            activeBuffs.Remove(type);
    }
}
