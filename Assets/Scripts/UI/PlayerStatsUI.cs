using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStatsUI : MonoBehaviour, IPlayerStatUI
{
    [SerializeField] TextMeshProUGUI playerNameDisplay;
    [SerializeField] TextMeshProUGUI playerClassDisplay;
    [SerializeField] TextMeshProUGUI playerCurrentLevelDisplay;
    [SerializeField] TextMeshProUGUI playerExpDisplay;
    [SerializeField] TextMeshProUGUI playerUpgradeStatsRemainingDisplay;
    [SerializeField] TextMeshProUGUI playerHealthStatsDisplay;
    [SerializeField] TextMeshProUGUI playerDefenseStatsDisplay;
    [SerializeField] TextMeshProUGUI playerCritRateStatsDisplay;
    [SerializeField] TextMeshProUGUI playerCritDamageStatsDisplay;
    [SerializeField] TextMeshProUGUI playerBonusDamageStatsDisplay;
    
    [Header("Color")]
    [SerializeField] private Color normalColor;
    [SerializeField] private Color bonusColor;
    [SerializeField] private Color debonusColor;

    [Header("Class Change")]
    [SerializeField] private ChangeClassUI changeClassUI;

    private PlayerStats playerStats;
    private Player player;
    private bool inventorySubscribed;

    private void OnEnable()
    {
        SubscribeInventory();
        RefreshFromBestAvailable();
    }

    private void OnDisable()
    {
        UnsubscribePlayerStats();
        UnsubscribeInventory();

        if (changeClassUI != null)
            changeClassUI.Hide();
    }

    public void SetPlayerData(GameObject playerObject)
    {
        if (playerObject == null) return;

        player = playerObject.GetComponent<Player>();
        AttachPlayerStats(playerObject.GetComponent<PlayerStats>());
        RefreshFromPlayer();
    }

    public void RefreshFromSelectedProfile()
    {
        var profile = GameManager.Instance != null ? GameManager.Instance.getCurrentSaveProfileData() : null;
        if (profile == null)
            return;

        RefreshFromSaveProfile(profile);
    }

    private void RefreshFromBestAvailable()
    {
        if (playerStats != null)
        {
            RefreshFromPlayer();
            return;
        }

        if (GameManager.Instance != null && GameManager.Instance.localPlayer != null)
        {
            player = GameManager.Instance.localPlayer;
            AttachPlayerStats(player.GetComponent<PlayerStats>());
            RefreshFromPlayer();
            return;
        }

        var profile = GameManager.Instance != null ? GameManager.Instance.getCurrentSaveProfileData() : null;
        if (profile != null)
        {
            RefreshFromSaveProfile(profile);
            return;
        }
    }

    private void RefreshFromPlayer()
    {
        if (playerStats == null)
            return;

        ClassType classType = playerStats.playerClass.Value;
        int level = playerStats.level.Value;
        BonusStats bonus = playerStats.bonusStats.Value;
        Stats relicStats = GetRelicStatsSafe();

        int exp = player != null ? player.experience : 0;
        int startExp;
        int nextLevelExp;
        GetLevelExpBounds(level, out startExp, out nextLevelExp);
        int upgradePoints = player != null ? player.upgradePoints : 0;
        string playerName = player != null ? player.characterName : "Player";

        UpdateUI(playerName, classType, level, exp, startExp, nextLevelExp, upgradePoints, bonus, relicStats);
        RefreshClassChangePanel();
    }

    private void RefreshFromSaveProfile(SaveProfileData profile)
    {
        if (profile == null)
            return;

        var save = profile.playerSaveData;
        Stats relicStats = GetRelicStatsSafe();
        int startExp;
        int nextLevelExp;
        GetLevelExpBounds(save.level, out startExp, out nextLevelExp);

        UpdateUI(
            save.characterName.ToString(),
            save.characterClass,
            save.level,
            save.experience,
            startExp,
            nextLevelExp,
            save.upgradePoints,
            save.bonusStats,
            relicStats
        );

        bool canChangeClass = save.level == 15;
        if(save.characterClass != ClassType.Human) 
            canChangeClass = false;
        if (changeClassUI != null)
        {
            if (canChangeClass)
                changeClassUI.Show();
            else
                changeClassUI.Hide();
        }
    }

    private void UpdateUI(
        string playerName,
        ClassType classType,
        int level,
        int experience,
        int startLevelExperience,
        int nextLevelExperience,
        int upgradePoints,
        BonusStats bonusStats,
        Stats relicStats
    )
    {
        if (playerNameDisplay != null) playerNameDisplay.text = playerName;
        if (playerClassDisplay != null) playerClassDisplay.text = classType.ToString();
        if (playerCurrentLevelDisplay != null) playerCurrentLevelDisplay.text = level.ToString();
        if (playerExpDisplay != null)
        {
            int start = Mathf.Max(0, experience - startLevelExperience);
            int needed = Mathf.Max(0, nextLevelExperience - startLevelExperience);
            // Debug.Log($"Exp{experience} Start Exp{startLevelExperience} nextLevelExperience {nextLevelExperience}");
            playerExpDisplay.text = $"{start} / {needed} exp";
        }
        if (playerUpgradeStatsRemainingDisplay != null)
            playerUpgradeStatsRemainingDisplay.text = $"Upgrade points : {upgradePoints.ToString()}";

        Stats baseStats = GetBaseStats(classType, level);
        Stats totalStats = baseStats + bonusStats;
        
        totalStats.health = (baseStats.health + bonusStats.bonusHealth) * (1 + relicStats.health);
        totalStats.defense = (baseStats.defense + bonusStats.bonusDefense) * (1 + relicStats.defense);
        totalStats.critRate += relicStats.critRate;
        totalStats.critDamage += relicStats.critDamage;
        totalStats.extraDamage += relicStats.extraDamage;

        if (playerHealthStatsDisplay != null)
            playerHealthStatsDisplay.text = FormatStat(baseStats.health + bonusStats.bonusHealth, totalStats.health);

        if (playerDefenseStatsDisplay != null)
            playerDefenseStatsDisplay.text = FormatStat(baseStats.defense + bonusStats.bonusDefense, totalStats.defense);

        if (playerCritRateStatsDisplay != null)
            playerCritRateStatsDisplay.text = FormatPercentStat(baseStats.critRate, totalStats.critRate);

        if (playerCritDamageStatsDisplay != null)
            playerCritDamageStatsDisplay.text = FormatPercentStat(baseStats.critDamage, totalStats.critDamage);

        if (playerBonusDamageStatsDisplay != null)
            playerBonusDamageStatsDisplay.text = FormatBonusDamageStat(baseStats.extraDamage, totalStats.extraDamage);
    }

    private Stats GetBaseStats(ClassType classType, int level)
    {
        ClassStatData data = GameDatabase.Instance.GetClassDatabase()
            .GetClassStatData(classType);

        if (data == null)
            return new Stats();

        return new Stats
        {
            health = data.GetHealth(level),
            defense = data.GetDefense(level),
            critRate = data.GetCritRate(level),
            critDamage = data.GetCritDamage(level),
            // Extra damage is treated as a multiplier; base is 1.0 (100%).
            extraDamage = 1.0f
        };
    }

    private Stats GetRelicStatsSafe()
    {
        if (InventoryManager.Instance == null)
            return new Stats();

        return InventoryManager.Instance.GetRelicStats();
    }

    private void RefreshClassChangePanel()
    {
        if (changeClassUI == null)
            return;

        bool canChangeClass = player != null && player.playerExperience != null && player.playerExperience.CanChangeClass();
        if (canChangeClass && playerStats != null)
        {
            changeClassUI.Show();
        }
        else
        {
            changeClassUI.Hide();
        }
    }

    private void GetLevelExpBounds(int level, out int startExp, out int nextLevelExp)
    {
        AnimationCurve curve = GameDatabase.Instance.GetExperienceData().experienceCurve;
        if (curve == null)
        {
            Debug.LogError("cannot Found experience curve");
            startExp = 0;
            nextLevelExp = 0;
            return;
        }
        if(level == 0)
        {
            startExp = 0;
            nextLevelExp = 0;
            return;
        }
        startExp = Mathf.RoundToInt(curve.Evaluate(level));
        nextLevelExp = Mathf.RoundToInt(curve.Evaluate(level + 1));
        // Debug.Log($"Start Exp{startExp}");
        // Debug.Log($"Start Exp{nextLevelExp}");
    }

    private void AttachPlayerStats(PlayerStats stats)
    {
        if (playerStats == stats)
            return;

        UnsubscribePlayerStats();
        playerStats = stats;

        if (playerStats == null)
            return;

        playerStats.level.OnValueChanged += OnLevelChanged;
        playerStats.playerClass.OnValueChanged += OnClassChanged;
        playerStats.bonusStats.OnValueChanged += OnBonusStatsChanged;
    }

    private void UnsubscribePlayerStats()
    {
        if (playerStats == null)
            return;

        playerStats.level.OnValueChanged -= OnLevelChanged;
        playerStats.playerClass.OnValueChanged -= OnClassChanged;
        playerStats.bonusStats.OnValueChanged -= OnBonusStatsChanged;
        playerStats = null;
        player = null;
    }

    private void OnLevelChanged(int oldValue, int newValue)
    {
        RefreshFromPlayer();
    }

    private void OnClassChanged(ClassType oldValue, ClassType newValue)
    {
        RefreshFromPlayer();
    }

    private void OnBonusStatsChanged(BonusStats oldValue, BonusStats newValue)
    {
        RefreshFromPlayer();
    }

    private void SubscribeInventory()
    {
        if (inventorySubscribed || InventoryManager.Instance == null)
            return;

        InventoryManager.Instance.OnEquipmentChanged += OnEquipmentChanged;
        inventorySubscribed = true;
    }

    private void UnsubscribeInventory()
    {
        if (!inventorySubscribed || InventoryManager.Instance == null)
            return;

        InventoryManager.Instance.OnEquipmentChanged -= OnEquipmentChanged;
        inventorySubscribed = false;
    }

    private void OnEquipmentChanged()
    {
        RefreshFromBestAvailable();
    }

    string FormatStat(float baseStat, float totalStat)
    {
        float bonus = totalStat - baseStat;
        // print("Health Bonus: " + bonus + " Total: " + totalStat + " Base: " + baseStat);
        string normalHex = ColorUtility.ToHtmlStringRGB(normalColor);
        string bonusHex = ColorUtility.ToHtmlStringRGB(bonusColor);
        string debonusHex = ColorUtility.ToHtmlStringRGB(debonusColor);

        string baseText = $"<color=#{normalHex}>{baseStat:0.##}</color>";

        if (Mathf.Approximately(bonus, 0))
            return baseText;

        string color = bonus > 0 ? bonusHex : debonusHex;

        return $"{baseText} <color=#{color}>({bonus:+0.##;-0.##})</color>";
    }

    string FormatPercentStat(float baseStat, float totalStat)
    {
        float basePct = baseStat * 100f;
        float totalPct = totalStat * 100f;
        float bonus = totalPct - basePct;

        string normalHex = ColorUtility.ToHtmlStringRGB(normalColor);
        string bonusHex = ColorUtility.ToHtmlStringRGB(bonusColor);
        string debonusHex = ColorUtility.ToHtmlStringRGB(debonusColor);

        string baseText = $"<color=#{normalHex}>{basePct:0.##}%</color>";

        if (Mathf.Approximately(bonus, 0))
            return baseText;

        string color = bonus > 0 ? bonusHex : debonusHex;

        return $"{baseText} <color=#{color}>({bonus:+0.##;-0.##}%)</color>";
    }

    string FormatBonusDamageStat(float baseMultiplier, float totalMultiplier)
    {
        // extraDamage is a multiplier (1.0 = 0% bonus). Display bonus percentage.
        float basePct = (baseMultiplier - 1f) * 100f;
        float totalPct = (totalMultiplier - 1f) * 100f;
        return FormatPercentStat(basePct / 100f, totalPct / 100f);
    }
}
