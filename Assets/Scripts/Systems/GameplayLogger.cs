using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;

public class GameplayLogger : MonoBehaviour
{
    public static GameplayLogger Instance;

    public float logInterval = 0.5f;

    private float timer;
    private bool isLogging = false;

    private List<string> logs = new List<string>();

    private void Awake()
    {
        // ✅ Singleton + DontDestroy
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        SceneManager.sceneLoaded += OnSceneLoaded;
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnDestroy()
    {
        // ✅ Prevent event leak
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        bool isGameScene = scene.name.Contains("_0"); // 🔥 adjust

        if (isGameScene && !isLogging)
        {
            StartLogging();
        }
        else if (!isGameScene && isLogging)
        {
            StopAndSave();
        }
    }

    void StartLogging()
    {
        logs.Clear();
        timer = 0f;
        isLogging = true;

        Debug.Log("📊 Start Logging");
    }

    void StopAndSave()
    {
        isLogging = false;
        SaveToFile();

        Debug.Log("💾 Stop & Saved");
    }

    void Update()
    {
        if (!isLogging) return;

        timer += Time.unscaledDeltaTime;

        if (timer >= logInterval)
        {
            timer = 0f;

            float fps = 1f / Time.smoothDeltaTime;

            int bullets = NetworkObjectPool.Singleton != null
                ? NetworkObjectPool.Singleton.ActiveObjectsCount
                : 0;

            logs.Add($"{Time.time:F2},{fps:F2},{bullets}");
        }
    }

    void SaveToFile()
    {
        if (logs.Count == 0) return;

        string filename = $"perf_{SystemInfo.deviceName}_{System.DateTime.Now:HHmmss}.csv";
        string path = Path.Combine(Application.persistentDataPath, filename);

        List<string> output = new List<string>();
        output.Add("Time,FPS,Bullets");
        output.AddRange(logs);

        File.WriteAllLines(path, output);

        Debug.Log("Saved: " + path);
    }
}