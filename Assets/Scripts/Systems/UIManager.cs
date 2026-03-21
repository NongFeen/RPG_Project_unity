using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }
    [SerializeField] private GameObject PlayerHUDUI;
    [SerializeField] private GameObject SettingMenu;
    [SerializeField] public GameObject dialoguePanel;
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
    private void OnDestroy()
    {
        if (inputReader != null)
        {
            inputReader.EscapeKey -= DoPopStackUI;
        }
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
        if (!isPress) return;

        //has stack
        if (menuStack.Count > 0)
        {
            CloseTopMenu();
            return;
        }

        //no stack. open setting menu
        // work good in play scene
        if (SettingMenu != null)
        {
            OpenMenu(SettingMenu);
        }
    }
    public void CloseTopMenu()
    {
        if (menuStack.Count == 0) return;

        GameObject top = menuStack.Pop();
        top.SetActive(false);
    }
    public void CloseAllMenus()
    {
        while (menuStack.Count > 0)
        {
            GameObject top = menuStack.Pop();
            if (top != null)
            {
                top.SetActive(false);
            }
        }
    }
}
