using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class LobbyManager : MonoBehaviour
{
    public static LobbyManager Instance;
    [SerializeField] public GameObject lobbyMenu;
    // [SerializeField] public GameObject joinMenu;
    [SerializeField] public GameObject selectCharacterMenu;
    [SerializeField] private string defaultIP = "127.0.0.1";
    [SerializeField] private ushort defaultPort = 7777;
    [SerializeField] private List<PlayerSaveData> playersSaveDatas;
    [SerializeField] private TextMeshProUGUI IPText;
    [SerializeField] private TextMeshProUGUI PortText;
    
    private UnityTransport transport;

    void Start()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
    }
    public void ShowLobbyMenu()
    {
        lobbyMenu.SetActive(true);
    }
    public void CloseLobbyMenu()
    {
        lobbyMenu.SetActive(false);
    }
    // ------------------------------
    // HOST LOBBY
    // ------------------------------
    public void HostLobby(string ip = null, ushort? port = null)
    {
        GameManager.Instance.gameState = GameState.Lobby;
        ip ??= defaultIP;
        port ??= defaultPort;

        // Configure Transport for host
        transport.ConnectionData.Address = ip;
        transport.ConnectionData.Port = port.Value;

        bool started = NetworkManager.Singleton.StartHost();
        Debug.Log("HOST STARTED: " + started + " at " + ip + ":" + port);
    }
    public void HostDefaultLobby()
    {
        GameManager.Instance.gameState = GameState.Lobby;

        string ip = defaultIP;
        ushort port = defaultPort;

        // Configure Transport for host
        transport.ConnectionData.Address = ip;
        transport.ConnectionData.Port = port;
        bool started = NetworkManager.Singleton.StartHost();
        Debug.Log("HOST STARTED: " + started + " at " + ip + ":" + port);
    }
    // ------------------------------
    // JOIN LOBBY
    // ------------------------------
    public bool JoinLobby(string ip, ushort port)
    {
        // Configure Transport for client
        string connectIP = IsValidIP(ip) ? ip: defaultIP;
        ushort connectPort = IsValidPort(port) ? port : defaultPort;

        transport.ConnectionData.Address = connectIP;
        transport.ConnectionData.Port = connectPort;

        Debug.Log("CLIENT CONNECTING TO " + connectIP + ":" + connectPort);
        bool started = NetworkManager.Singleton.StartClient();
        if (started)
        {
            Debug.Log("CLIENT Connected");
            GameManager.Instance.gameState = GameState.Lobby;
            return true;
        }
        else
        {
            Debug.LogError("CLIENT Failed to connect");
            return false;
        }
    }
    private void ShowSelectCharacterMenu()
    {
        selectCharacterMenu.gameObject.SetActive(true);
    }
    public void LeaveLobby()
    {
        if (NetworkManager.Singleton.IsClient || NetworkManager.Singleton.IsHost)
        {
            NetworkManager.Singleton.Shutdown();
            Debug.Log("Disconnected from lobby");
        }
    }
    // public void OnTargetIPPortChanged()
    // {
    //     string ip = IPText.text.Trim();
    //     // ip = Regex.Replace(ip, @"[^\d]", "");
    //     ip = ip.Replace("\u200B", "");
        
    //     string port = PortText.text.ToString().Trim(); 
    //     port = Regex.Replace(port, @"[^\d]", "");

    //     TargetIP = ip;
    //     if(port!="")
    //         TargetPort = ushort.Parse(port);
    // }
    private bool IsValidIP(string ip)
    {
        return !string.IsNullOrWhiteSpace(ip);
    }
    private bool IsValidPort(ushort port)
    {
        return port != 0;
    }
    
}
