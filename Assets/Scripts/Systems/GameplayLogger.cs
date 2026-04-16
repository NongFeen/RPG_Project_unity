using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameplayLogger : MonoBehaviour
{
    public static GameplayLogger Instance;

    private bool isLogging = false;
    private List<string> logs = new List<string>();
    
    // Buffer for FrameTiming API
    private FrameTiming[] m_FrameTimings = new FrameTiming[1];

    private void Awake()
    {
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
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Adjust naming check to match your game scene
        if (scene.name.Contains("_0") && !isLogging)
        {
            StartLogging();
        }
        else if (!scene.name.Contains("_0") && isLogging)
        {
            StopAndSave();
        }
    }

    void StartLogging()
    {
        logs.Clear();
        isLogging = true;
        Debug.Log("📊 High-Res Logging Started (Every Frame)");
    }

    void StopAndSave()
    {
        isLogging = false;
        SaveToFile();
    }

    // Now running every frame for maximum data precision
    void Update()
    {
        if (!isLogging) return;

        // 1. Capture timings for the frame that JUST finished
        FrameTimingManager.CaptureFrameTimings();
        uint frameCount = FrameTimingManager.GetLatestTimings(1, m_FrameTimings);

        if (frameCount > 0)
        {
            FrameTiming frame = m_FrameTimings[0];

            float cpuMs = (float)frame.cpuFrameTime;
            float gpuMs = (float)frame.gpuFrameTime;
            float frameTimeMs = Mathf.Max(cpuMs, gpuMs);
            float fps = 1000f / frameTimeMs;// 1000 ms = 1 sec

            int bullets = NetworkObjectPool.Singleton != null
                ? NetworkObjectPool.Singleton.ActiveObjectsCount
                : 0;

            logs.Add($"{Time.time:F4},{fps:F2},{cpuMs:F3},{gpuMs:F3},{bullets}");
        }
    }

    void SaveToFile()
    {
        if (logs.Count == 0) return;

        // 1. Collect Hardware and OS Info
        string cpu = SystemInfo.processorType;
        string gpu = SystemInfo.graphicsDeviceName;
        string os = SystemInfo.operatingSystem;
        int ram = SystemInfo.systemMemorySize; // In MB

        // 2. Sanitize all strings for file system compatibility
        string safeCpu = SanitizePath(cpu);
        string safeGpu = SanitizePath(gpu);
        string safeOs = SanitizePath(os);

        // 3. Construct the filename
        // Example: perf_20240520_1430_Win11_Ryzen9_RTX5090_65536MB.csv
        string filename = $"perf_{safeOs}_{safeCpu}_{safeGpu}_{ram}MB.csv";
        
        string path = Path.Combine(Application.persistentDataPath, filename);

        // 4. Prepare CSV content
        List<string> output = new List<string>();
        output.Add("Time,FPS,CPUMs,GPUMs,Bullets");
        output.AddRange(logs);

        try
        {
            File.WriteAllLines(path, output);
            Debug.Log($"<color=cyan>📊 Log Saved:</color> {path}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to save performance log: {e.Message}");
        }
    }
    private string SanitizePath(string dirtyString)
    {
        // Remove standard invalid file characters
        foreach (char c in Path.GetInvalidFileNameChars())
        {
            dirtyString = dirtyString.Replace(c, '_');
        }

        // Clean up common hardware/OS string clutter
        return dirtyString
            .Replace(" ", "_")
            .Replace("(", "")
            .Replace(")", "")
            .Replace(".", "_")
            .Replace("__", "_"); // Clean up double underscores
    }
}