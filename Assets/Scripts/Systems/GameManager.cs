using System;
using System.Collections.Generic;
using NUnit.Framework;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using static SaveSystem;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance { get; private set; }
    public bool isOpenInventory = false;
    [SerializeField] private InputReader inputReader;
    [SerializeField] private GameObject inventoryCanvasUI;
    [SerializeField] public GameObject gameCompleteUIPrefab;
    [SerializeField] private SaveProfileData selectedSaveProfileData;
    [SerializeField] public Player localPlayer;
    [SerializeField] public MapName selectMapName;
    [SerializeField] public GameState gameState;
    bool IsConnected =>
    NetworkManager.Singleton != null &&
    NetworkManager.Singleton.IsClient &&
    NetworkManager.Singleton.IsConnectedClient;
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        gameState = GameState.MainMenu;
        // if (inventoryCanvasUI != null)
        // {
        //     // DontDestroyOnLoad(inventoryCanvasUI);
        // }
    }
    public void Update()
    {
        // print(selectedSaveProfileData.playerSaveData.characterName);   
    }
    private void OnEnable()
    {
        // print("GameManager Subscribe to OpenEventory");
        inputReader.OpenInventoryEvents += OpenInventoryEvents;
    }
    private void OnDisable()
    {
        // print("GameManager Unsubscribe to OpenEventory");ไ
        inputReader.OpenInventoryEvents -= OpenInventoryEvents;
    }
    private void OpenInventoryEvents(bool isPressed)
    {
        // print("Open Inventory");//asdas
        if (!isPressed) return;
        isOpenInventory = !isOpenInventory; // toggle true/false
        inventoryCanvasUI.SetActive(isOpenInventory);
    }
    public void Save()
    {
        SaveSystem.Save();
        print("Saving Data");
    }
    public SaveProfileData getCurrentSaveProfileData()
    {
        return selectedSaveProfileData;
    }
    
    public void SelectPlayer(SaveProfileData saveProfileData)
    {
        selectedSaveProfileData = saveProfileData;
        InventoryManager.Instance.LoadInventoryFromSaveData(selectedSaveProfileData.itemSaveData);
        // when select profile for host/join lobby, send save data to LobbyNetwork playersProfileData
        if (IsConnected)
        {
            LobbyNetwork.Instance.SubmitPlayerProfileServerRpc(selectedSaveProfileData.playerSaveData);
        }
    }
    public void StartGame(MapName map)
    {
        LoadingScreenManager.Instance.LoadScene(map.ToString());
    }
    public void SelectMap(MapName map)
    {
        this.selectMapName = map;
        if (LobbyNetwork.Instance && IsHost)
        {
            LobbyNetwork.Instance.HostSelectMap(map);
        }
    }
    public void SetLocalPlayer(Player localPlayer)
    {
        this.localPlayer = localPlayer;
    }
    public void OnGameComplete(int experienceGained, List<WeaponInstance> dropsItems, List<RelicInstance> relicDrops)
    {
        //add exp and item to player
        localPlayer.AddExperience(experienceGained);
        dropsItems.ForEach((dropsItems) => InventoryManager.Instance.AddItemInstance(dropsItems));
        if (relicDrops != null)
        {
            relicDrops.ForEach((relic) => InventoryManager.Instance.AddRelicInstance(relic));
        }
        //show what added
        GameObject ui = Instantiate(gameCompleteUIPrefab);
        ui.TryGetComponent<GameSummary>(out var gameSummary);
        gameSummary.ShowSummary(experienceGained, dropsItems, relicDrops);
        Save();
    }

    [ClientRpc]
    public void RequestEndSessionClientRpc()
    {
        print("Received request to end session");
        NetworkManager.Singleton.Shutdown();
        GoToMainMenu();
    }
    public void GoToMainMenu()
    {
        UIManager.Instance.DeactivePlayerHUD();
        // UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
        LoadingScreenManager.Instance.LoadScene("MainMenu");
        GameManager.Instance.gameState = GameState.Lobby;
        NetworkManager.Singleton.Shutdown();
    }
}