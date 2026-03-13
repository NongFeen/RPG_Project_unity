using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }
    [SerializeField] private GameObject PlayerHUDUI;
    [SerializeField] private Stack<GameObject> menuStack = new Stack<GameObject>();
    [SerializeField] InputReader inputReader;
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        // DontDestroyOnLoad(gameObject);
        inputReader.EscapeKey += DoPopStackUI;
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
    
    public void OpenMenu(GameObject menu)
    {
        menu.SetActive(true);
        menuStack.Push(menu);
    }
    public void DoPopStackUI(bool isPress)
    {
        if(isPress) CloseTopMenu();
    }
    public void CloseTopMenu()
    {
        if (menuStack.Count == 0) return;

        GameObject top = menuStack.Pop();
        top.SetActive(false);
    }
}
