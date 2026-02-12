using Unity.Netcode;
using Unity.Services.Matchmaker.Models;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class JoinSever : MonoBehaviour
{
    public void JoinServer()
    {
        Debug.Log("Joining Server");
        // SceneManager.LoadScene("SampleScene");
        NetworkManager.Singleton.StartClient();
    }

    public void StartServer()
    {
        // SceneManager.LoadScene("SampleScene");
        Debug.Log("Hosting Server");
        NetworkManager.Singleton.StartHost();
        NetworkManager.Singleton.SceneManager.LoadScene("PlayScene", LoadSceneMode.Single);
    }
}
