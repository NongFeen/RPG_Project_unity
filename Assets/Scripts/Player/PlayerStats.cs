using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using Unity.Collections;

public class PlayerStats : NetworkBehaviour
{
    [Header("Stats (Network Synced)")]
    public NetworkVariable<FixedString32Bytes> playerName = 
        new NetworkVariable<FixedString32Bytes>("Name",
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);
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
    public NetworkVariable<bool> isGhost =
        new NetworkVariable<bool>(false,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);

    //bonus stats from player upgrades
    public NetworkVariable<BonusStats> bonusStats =
        new NetworkVariable<BonusStats>(new BonusStats(),
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);

    public NetworkVariable<Stats> relicStats =
        new NetworkVariable<Stats>(new Stats(),
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
    public bool IsGhost => isGhost.Value;
    public override void OnNetworkSpawn()
    {
        if (PlayerManager.Instance != null)
            PlayerManager.Instance.RegisterPlayer(this);
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
        isGhost.OnValueChanged += OnGhostChanged;
        ApplyGhostState(isGhost.Value);
    }
    public override void OnNetworkDespawn()
    {
        if (PlayerManager.Instance != null)
            PlayerManager.Instance.UnregisterPlayer(this);
        if (IsOwner)
        {
            currentHP.OnValueChanged -= OnHPChanged;
            UIManager.Instance.DeactivePlayerHUD();
        }
        isGhost.OnValueChanged -= OnGhostChanged;
    }
    public void Update()
    {
        // The server owns gameplay stats while the owning client owns presentation.
        // Other clients do not need to simulate this player's buff timers.
        if (!IsServer && !IsOwner) return;

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

        if (toRemove.Count > 0 && IsServer)
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
        playerName.Value = save.characterName;
        Debug.Log($"Loaded save for {clientId} : Name {save.characterName.ToString()}");
    }
    public void TakeDamage(float amount)
    {
        if (!IsServer) return;
        if (isGhost.Value) return;
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
        if (isGhost.Value) return;
        Debug.Log($"{OwnerClientId} has died");
        isGhost.Value = true;
    }
    private void OnGhostChanged(bool oldValue, bool newValue)
    {
        ApplyGhostState(newValue);
    }
    private void ApplyGhostState(bool ghosted)
    {
        Collider2D[] colliders = GetComponentsInChildren<Collider2D>(true);
        foreach (var col in colliders)
        {
            col.enabled = !ghosted;
        }
    }
    public ClassType GetClassType()
    {
        return playerClass.Value;
    }
    public void RecalculateStableStats()
    {
        ClassStatData data = GameDatabase.Instance.GetClassDatabase().GetClassStatData(playerClass.Value);

        if (data == null)
        {
            Debug.LogError("No ClassStatData for " + playerClass.Value);
            return;
        }
        Debug.Log($"Current class {playerClass.Value} + {data.classType}");
        int lvl = level.Value;
        Stats calStats;
        calStats.health = data.GetHealth(lvl);
        calStats.defense = data.GetDefense(lvl);
        calStats.critRate = data.GetCritRate(lvl);
        calStats.critDamage = data.GetCritDamage(lvl);
        calStats.extraDamage = 1.0f;
        //bonus stats is flat hp
        calStats += bonusStats.Value;

        // because of case HP% cannot do flat plus
        calStats.health *= 1+relicStats.Value.health;
        calStats.critRate += relicStats.Value.critRate;
        calStats.critDamage += relicStats.Value.critDamage;
        calStats.defense += relicStats.Value.defense;
        stableStats.Value = calStats;   
        
        //display server stats for debugging
        // print($"Base Stat : {data.GetHealth(lvl)}, {data.GetDefense(lvl)}, {data.GetCritRate(lvl)}, {data.GetCritDamage(lvl)}");
        // print($"Bonus Stat: {bonusStats.Value}");
        // print($"Active Stat Level {lvl}: {stableStats.Value}");
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

    [ServerRpc]
    public void ChangeClassServerRpc(ClassType newClass)
    {
        // only allow class change at level 15
        if (level.Value != 15)
            return;

        if (playerClass.Value == newClass)
            return;
        playerClass.Value = newClass;
        RecalculateStableStats();
        RecalculateActiveStats();

        Debug.Log($"Player class changed to {newClass}");
    }

    [ServerRpc]
    public void ApplyUpgradeServerRpc(StatType stat)
    {
        //upgrade while game is progess
        BonusStats newBonus = bonusStats.Value;
        switch (stat)
        {
            case StatType.Health:
                newBonus.bonusHealth += 10;
                break;

            case StatType.Defense:
                newBonus.bonusDefense += 3;
                break;

            case StatType.CritChance:
                newBonus.bonusCritChance += 0.01f;
                break;

            case StatType.CritDamage:
                newBonus.bonusCritDamage += 0.02f;
                break;
        }

        bonusStats.Value = newBonus;
        
        // calculate hp percent before apply new hp
        float hpPercent = currentHP.Value/activeStats.Value.health;

        RecalculateStableStats();
        RecalculateActiveStats();
        currentHP.Value = hpPercent * activeStats.Value.health;
        
    }
    [ServerRpc]
    public void ChangeLevelServerRpc(int level){
        this.level.Value = level;
        RecalculateStableStats();
        RecalculateActiveStats();
        Debug.Log($"Player level changed to {level}");
    }
    [ServerRpc]
    public void SetRelicStatsServerRpc(Stats newRelicStats)
    {
        relicStats.Value = newRelicStats;

        float hpPercent = currentHP.Value / activeStats.Value.health;
        RecalculateStableStats();
        RecalculateActiveStats();
        currentHP.Value = hpPercent * activeStats.Value.health;
    }
}
