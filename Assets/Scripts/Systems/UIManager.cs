using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }
    [SerializeField] private GameObject PlayerHUDUI;
    [SerializeField] private GameObject SettingMenu;
    [SerializeField] public GameObject dialoguePanel;
    [SerializeField] public Stack<GameObject> menuStack = new Stack<GameObject>();
    // Tracks menus opened as a group so one ESC can close them all.
    private readonly Dictionary<GameObject, List<GameObject>> groupedMenus = new Dictionary<GameObject, List<GameObject>>();
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
    public void OpenMenuGroup(List<GameObject> menus)
    {
        if (menus == null || menus.Count == 0) return;

        GameObject root = null;
        var groupList = new List<GameObject>();
        foreach (var menu in menus)
        {
            if (menu == null) continue;
            menu.SetActive(true);
            groupList.Add(menu);
            if (root == null)
            {
                root = menu;
            }
        }
        if (root == null || groupList.Count == 0) return;

        menuStack.Push(root);
        groupedMenus[root] = groupList;
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

        if (SettingMenu != null)
        {
            OpenMenu(SettingMenu);
        }
    }
    public void CloseTopMenu()
    {
        if (menuStack.Count == 0) return;

        GameObject top = menuStack.Pop();
        if (top == null) return;

        //for open multiple menu at once
        if (groupedMenus.TryGetValue(top, out var groupList))
        {
            for (int i = 0; i < groupList.Count; i++)
            {
                var menu = groupList[i];
                if (menu != null)
                {
                    menu.SetActive(false);
                }
                menu.TryGetComponent<LobbyUI>(out var lobbyUI);
                if(lobbyUI !=null) lobbyUI.ExitLobby();
            }
            groupedMenus.Remove(top);
            return;
        }
        top.SetActive(false);
        top.TryGetComponent<LobbyUI>(out var lobby);
            if(lobby !=null) lobby.ExitLobby();
    }
    public void CloseAllMenus()
    {
        while (menuStack.Count > 0)
        {
            GameObject top = menuStack.Pop();
            if (top == null) continue;
            if (groupedMenus.TryGetValue(top, out var groupList))
            {
                for (int i = 0; i < groupList.Count; i++)
                {
                    var menu = groupList[i];
                    if (menu != null)
                    {
                        menu.SetActive(false);
                    }
                }
                groupedMenus.Remove(top);
                continue;
            }
            top.SetActive(false);
        }
    }
}
