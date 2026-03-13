using UnityEngine;

public class UpgradeBonusStats : MonoBehaviour
{
    [SerializeField] private Player player;
    [SerializeField] private PlayerStats playerStats;

    private void Awake()
    {
        TryResolveRefs();
    }

    private void OnEnable()
    {
        TryResolveRefs();
    }

    private void TryResolveRefs()
    {
        if (player == null && GameManager.Instance != null)
            player = GameManager.Instance.localPlayer;

        if (playerStats == null && player != null)
            playerStats = player.GetComponent<PlayerStats>();
    }

    public void UpgradeHealth()
    {
        Upgrade(StatType.Health);
    }

    public void UpgradeDefense()
    {
        Upgrade(StatType.Defense);
    }

    public void UpgradeCritChance()
    {
        Upgrade(StatType.CritChance);
    }

    public void UpgradeCritDamage()
    {
        Upgrade(StatType.CritDamage);
    }

    private void Upgrade(StatType stat)
    {
        if (player == null || playerStats == null)
            return;

        if (!player.SpendUpgradePoint(stat))
            return;

        playerStats.ApplyUpgradeServerRpc(stat);
    }
}
