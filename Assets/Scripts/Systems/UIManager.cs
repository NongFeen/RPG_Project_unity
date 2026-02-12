using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }
    [SerializeField] private GameObject PlayerHUDUI;
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        // DontDestroyOnLoad(gameObject);
    }
    public void ActivePlayerHUD(PlayerStats playerStats,PlayerEquipedItem playerEquipedItem)
    {
        PlayerHUDUI.SetActive(true);
        PlayerHUDUI.TryGetComponent<PlayerGameplayUI>(out var playerGameplayUI);
        playerGameplayUI.SetPlayerData(playerStats, playerEquipedItem);
    }
    public void DeactivePlayerHUD()
    {
        PlayerHUDUI.SetActive(false);
    }
}
