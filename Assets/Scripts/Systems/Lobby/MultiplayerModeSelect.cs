using System.Text.RegularExpressions;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class MultiplayerModeSelect : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI IPText;
    [SerializeField] private TextMeshProUGUI PortText;
    [SerializeField] public string TargetIP;
    [SerializeField] public ushort TargetPort;
    [SerializeField] public GameObject joinMenu;
    [SerializeField] public GameObject selectCharacterMenu;
    // [SerializeField] public GameObject lobbyMenu;

    public void CreateServer()
    {
        LobbyNetwork.Instance.ResetPlayersProfileData();
        LobbyManager.Instance.HostDefaultLobby();
    }
    public void JoinServer()
    {
        if (LobbyManager.Instance.JoinLobby(TargetIP, TargetPort))
        {
            // connected
            joinMenu.SetActive(false);
            selectCharacterMenu.SetActive(true);
            // lobbyMenu.SetActive(true);
            GameManager.Instance.gameState = GameState.Lobby;
        }
    }
    public void OnTargetIPPortChanged()
    {
        string ip = IPText.text.Trim();
        // ip = Regex.Replace(ip, @"[^\d]", "");
        ip = ip.Replace("\u200B", "");
        
        string port = PortText.text.ToString().Trim(); 
        port = Regex.Replace(port, @"[^\d]", "");

        TargetIP = ip;
        if(port!="")
            TargetPort = ushort.Parse(port);
    }
}
