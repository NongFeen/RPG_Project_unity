using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthUI : MonoBehaviour,IPlayerStatUI
{
    [SerializeField] TextMeshProUGUI hpText;
    [SerializeField] TextMeshProUGUI hpNumberText;
    [SerializeField] private PlayerStats playerStats;
    [SerializeField] private Slider HPbar;
    public void SetPlayerData(GameObject player)
    {
        playerStats = player.GetComponent<PlayerStats>();
        UpdateHp(0, playerStats.currentHP.Value);
        playerStats.currentHP.OnValueChanged += UpdateHp;
        playerStats.activeStats.OnValueChanged += UpdateMaxHp;
    }
    private void OnDestroy()
    {
        if (playerStats != null)
            playerStats.currentHP.OnValueChanged -= UpdateHp;
    }
    private void UpdateHp(float oldValue, float newValue)
    {
        hpNumberText.text = $"{newValue:F2} / {playerStats.activeStats.Value.health:F2}";
        HPbar.value = newValue / playerStats.activeStats.Value.health;
    }
    private void UpdateMaxHp(Stats oldValue, Stats newValue)
    {
        hpNumberText.text = $"{playerStats.currentHP.Value:F2} / {newValue.health:F2}";
    }
}
