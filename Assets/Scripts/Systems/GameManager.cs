using System;
using System.Collections.Generic;
using Unity.Netcode;
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

    public void LoadSelectableMap(SaveProfileData saveFile)
    {
        selectedSaveProfileData = saveFile;
        if (selectedSaveProfileData != null)
            selectedSaveProfileData.EnsureDefaults();

        // If current selection is locked for this save, clamp to the highest unlocked map.
        if (selectedSaveProfileData != null && !IsMapUnlocked(selectMapName))
            selectMapName = selectedSaveProfileData.highestUnlockedMap;

        RefreshMapInvokerLocks();
    }

    public bool IsMapUnlocked(MapName mapName)
    {
        if (selectedSaveProfileData == null)
            return false;

        selectedSaveProfileData.EnsureDefaults();
        return (int)mapName <= (int)selectedSaveProfileData.highestUnlockedMap;
    }
    
    public void SelectPlayer(SaveProfileData saveProfileData)
    {
        LoadSelectableMap(saveProfileData);
        InventoryManager.Instance.LoadInventoryFromSaveData(selectedSaveProfileData.itemSaveData);
        // when select profile for host/join lobby, send save data to LobbyNetwork playersProfileData
        if (IsConnected)
        {
            LobbyNetwork.Instance.SubmitPlayerProfileServerRpc(selectedSaveProfileData.playerSaveData);
        }
    }
    public void StartGame(MapName map)
    {
        // Track current map for reward/unlock logic.
        selectMapName = map;

        if (!IsMapUnlocked(map))
        {
            selectedSaveProfileData.EnsureDefaults();
            MapName fallback = selectedSaveProfileData.highestUnlockedMap;
            Debug.LogWarning($"Map {map} is locked. Falling back to {fallback}.");
            map = fallback;
            selectMapName = map;
        }
        LoadingScreenManager.Instance.LoadScene(map.ToString());
    }
    public void SelectMap(MapName map)
    {
        if (!IsMapUnlocked(map))
        {
            Debug.LogWarning($"Map {map} is locked for this profile.");
            return;
        }
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
        UnlockMapsFromCompletion(selectMapName);
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

    private void RefreshMapInvokerLocks()
    {
        var invokers = FindObjectsByType<MapInvoker>(FindObjectsSortMode.None);
        for (int i = 0; i < invokers.Length; i++)
            invokers[i].RefreshLockState();
    }

    private void UnlockMapsFromCompletion(MapName completedMap)
    {
        if (selectedSaveProfileData == null)
            return;

        selectedSaveProfileData.EnsureDefaults();

        // Progression: Story -> Dungeon -> Raid (enum order in MapName).
        // If the player completes the current highest map, unlock the next one.
        if ((int)completedMap >= (int)selectedSaveProfileData.highestUnlockedMap)
        {
            int maxIndex = Enum.GetValues(typeof(MapName)).Length - 1;
            int nextIndex = Mathf.Clamp((int)completedMap + 1, 0, maxIndex);
            selectedSaveProfileData.highestUnlockedMap = (MapName)nextIndex;
        }

        RefreshMapInvokerLocks();
    }

    [ClientRpc]
    public void RequestEndSessionClientRpc()
    {
        print("Received request to end session");
        NetworkManager.Singleton.Shutdown();
        if (LoadingScreenManager.Instance != null)
        {
            LoadingScreenManager.Instance.LoadClientScene("MainMenu");
        }
        GameManager.Instance.gameState = GameState.Lobby;
    }
    public void GoToMainMenu()
    {
        UIManager.Instance.DeactivePlayerHUD();
        if (NetworkManager.Singleton == null)
        {
            if (LoadingScreenManager.Instance != null)
            {
                LoadingScreenManager.Instance.LoadClientScene("MainMenu");
            }
            GameManager.Instance.gameState = GameState.Lobby;
            return;
        }

        if (IsServer)
        {
            // Tell all clients to disconnect and load their local menu.
            RequestEndSessionClientRpc();
        }

        // disconnect and return to menu
        NetworkManager.Singleton.Shutdown();
        if (LoadingScreenManager.Instance != null)
        {
            LoadingScreenManager.Instance.LoadClientScene("MainMenu");
        }
        GameManager.Instance.gameState = GameState.Lobby;
    }
}
