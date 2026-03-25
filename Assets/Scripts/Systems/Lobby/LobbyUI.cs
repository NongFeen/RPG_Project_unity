using System;
using System.Text;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class LobbyUI : MonoBehaviour
{
    [SerializeField] private GameObject PlayerCharacterCardPrefab;
    [SerializeField] private GameObject PlayerList;
    [SerializeField] private TextMeshProUGUI Maps;
    public void OnEnable()
    {
        LobbyNetwork.Instance.playersProfileData.OnListChanged += RefreshPlayerList;
        LobbyNetwork.Instance.MapName.OnValueChanged += RefreshMapSelected;
        RefreshMapSelected(MapName.Story_01, LobbyNetwork.Instance.MapName.Value);
    }
    public void OnDisable()
    {
        LobbyNetwork.Instance.playersProfileData.OnListChanged -= RefreshPlayerList;
        LobbyNetwork.Instance.MapName.OnValueChanged -= RefreshMapSelected;
        // NetworkManager.Singleton.Shutdown(false);
        // print("ShutDown server");
    }
    private void RefreshMapSelected(MapName previousValue, MapName newValue)
    {
        Maps.text = newValue.ToString();
    }

    private void RefreshPlayerList(NetworkListEvent<LobbyPlayerData> changeEvent)
    {
        
        RefreshPlayerList();
    }

    public void RefreshPlayerList()
    {
        var playerLobbyData = LobbyNetwork.Instance.GetLobbyPlayerDatas();
        PrintPlayerList();
        ClearPlayerCard();
        for (int i = 0; i < playerLobbyData.Count; i++)
        {
            CreatePlayerCard(i, playerLobbyData[i]);
        }
    }
    public void CreatePlayerCard(int index, LobbyPlayerData lobbyPlayerData)
    {
        var card = Instantiate(PlayerCharacterCardPrefab, PlayerList.transform);
        card.GetComponent<CharacterCardUI>().SetUp(index, lobbyPlayerData);
    }
    private void ClearPlayerCard()
    {
        foreach (Transform child in PlayerList.transform)
            Destroy(child.gameObject);
    }
    private void PrintPlayerList()
    {
        var playerLobbyData = LobbyNetwork.Instance.GetLobbyPlayerDatas();
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("===== LOBBY PLAYER DATA =====");
        for (int i = 0; i < playerLobbyData.Count; i++)
        {
            var p = playerLobbyData[i];
            var s = p.saveData;

            sb.AppendLine($"Player [{i}]");
            sb.AppendLine($"  CharacterName : {s.characterName.ToString()}");
            sb.AppendLine($"  Level         : {s.level}");
            sb.AppendLine($"  Experience    : {s.experience}");
            sb.AppendLine($"  Class         : {s.characterClass}");
            sb.AppendLine($"  BonusStat     : {s.bonusStats}");
            sb.AppendLine("--------------------------------");
        }
        Debug.Log(sb.ToString());
    }
    public void StartGame()
    {
        if(!NetworkManager.Singleton.IsHost) return;
        MapName targetMap = LobbyNetwork.Instance.GetCurrentMapName();
        //make sure to let client know is start loading scene
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsHost)
        {
            LobbyNetwork.Instance.ShowLoadingClientRpc();
        }
        GameManager.Instance.StartGame(targetMap);
    }

    public void ExitLobby()
    {
        if (LobbyManager.Instance != null)
        {
            LobbyManager.Instance.LeaveLobby();
        }
        else if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.Shutdown();
        }
    }
}
