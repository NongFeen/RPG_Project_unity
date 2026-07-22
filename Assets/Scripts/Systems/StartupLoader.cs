using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartupLoader : MonoBehaviour
{
    private const int TargetFrameRate = 240;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void ConfigureFrameRate()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = TargetFrameRate;
    }

    // void Start()
    // {
    //     // SceneManager.LoadScene("MainMenu");
    //     LoadingScreenManager.Instance.LoadScene("MainMenu");
    //     print(MapDatabase.Instance.Get(MapName.Story_01).displayName);
    // }
    private IEnumerator Start()
    {
        yield return StartCoroutine(WaitForManagers());

        Debug.Log("✅ All managers loaded. Starting game...");

        // Continue startup here
        Startup();
    }
    private IEnumerator WaitForManagers()
    {
        yield return WaitFor(
            "NetworkManager",
            () => 
                NetworkManager.Singleton != null 
        );
        yield return WaitFor(
            "GameDatabase",
            () => GameDatabase.Instance != null
        );

        yield return WaitFor(
            "LoadingScreenManager",
            () => LoadingScreenManager.Instance != null
        );

        yield return WaitFor(
            "LobbyNetwork",
            () => LobbyNetwork.Instance != null
        );

        yield return WaitFor(
            "GameManager",
            () => GameManager.Instance != null
        );

        yield return WaitFor(
            "InventoryManager",
            () => InventoryManager.Instance != null
        );
        yield return WaitFor(
            "SoundManager",
            () => SoundManager.Instance != null
        );
        
    }
    private IEnumerator WaitFor(string name, System.Func<bool> condition)
    {
        while (!condition())
        {
            yield return null;
        }

        Debug.Log($"🟢 {name} loaded");
    }
    void Startup()
    {
        // LoadingScreenManager.Instance.LoadScene("MainMenu");
        LoadingScreenManager.Instance.LoadClientScene("MainMenu");
    }
}
