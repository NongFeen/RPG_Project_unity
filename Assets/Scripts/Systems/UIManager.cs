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
    public void ActivePlayerHUD(GameObject player)
    {
        PlayerHUDUI.SetActive(true);
        PlayerHUDUI.TryGetComponent<PlayerGameplayUI>(out var playerGameplayUI);
        // playerGameplayUI.SetPlayerData(playerStats, playerEquipedItem);
        playerGameplayUI.SetPlayerData(player);
    }
    public void SetUpSkill(GameObject player)
    {
        PlayerHUDUI.TryGetComponent<PlayerGameplayUI>(out var playerGameplayUI);
        playerGameplayUI.SetPlayerSkill(player);
    }
    public void DeactivePlayerHUD()
    {
        PlayerHUDUI.SetActive(false);
    }
}
