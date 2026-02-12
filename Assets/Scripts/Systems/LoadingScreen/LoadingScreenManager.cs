using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingScreenManager : MonoBehaviour
{
    public static LoadingScreenManager Instance;

    [SerializeField] private GameObject loadingRoot;
    [SerializeField] private Slider progressBar;

    private NetworkSceneManager NetworkSceneManager => NetworkManager.Singleton.SceneManager;

    private Coroutine delayedShowRoutine;
    private bool isLoading;
    private float delayTime = 5;
    private bool isBinded = false;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        // DontDestroyOnLoad(gameObject);
        Hide();
    }

    private void OnDisable()
    {
        if (NetworkManager.Singleton == null) return;
        NetworkSceneManager.OnSceneEvent -= HandleOnSceneEvent;
        // SceneManager.OnLoad -= OnLoadStarted;
        // SceneManager.OnLoadEventCompleted -= OnLoadCompleted;
    }
    public void BindSceneEvents()
    {
        if (NetworkManager.Singleton == null )
        {
            Debug.LogWarning("NetworkManager is null, cannot bind scene events");
            return;
        }
        if(isBinded) return;

        // var sceneManager = NetworkManager.Singleton.SceneManager;
        // sceneManager.OnLoad += OnLoadStarted;
        NetworkManager.Singleton.SceneManager.OnSceneEvent += HandleOnSceneEvent;
        // sceneManager.OnLoadEventCompleted += OnLoadCompleted;
        isBinded = true;
        Debug.Log("subscribed to SceneManager events");
    }

    private async void HandleOnSceneEvent(SceneEvent sceneEvent)
    {
        if(NetworkManager.Singleton.LocalClientId != sceneEvent.ClientId)
            return;
        if(sceneEvent.SceneEventType == SceneEventType.Load)
        {
            Show();
            StartCoroutine(TrackProgress(sceneEvent.AsyncOperation));
        }
        if(sceneEvent.SceneEventType == SceneEventType.LoadComplete)
        {
            Hide();
        }
    }
    private IEnumerator TrackProgress(AsyncOperation op)
    {
        while (!op.isDone)
        {
            if (loadingRoot.activeSelf)
                progressBar.value = Mathf.Clamp01(op.progress / 0.9f);

            yield return null; 
        }
    }

    private IEnumerator DelayedShow()
    {
        yield return new WaitForSeconds(delayTime);

        if (isLoading)
            Show();
    }

    private void Show()
    {
        loadingRoot.SetActive(true);
        progressBar.value = 0f;
    }

    private void Hide()
    {
        loadingRoot.SetActive(false);
    }

    private void Complete()
    {
        progressBar.value = 1f;
    }

    // ---------- API ----------

    public void LoadScene(string sceneName)
    {
        BindSceneEvents();
        Show();
        if (!NetworkManager.Singleton.IsServer)
        {
            Debug.LogWarning("Only server can load scenes");
            return;
        }
        Debug.LogWarning("Loading scene" + NetworkSceneManager);
        NetworkSceneManager.LoadScene(sceneName, LoadSceneMode.Single);
    }
    public void LoadClientScene(string sceneName)
    {
        StartCoroutine(LoadClientSceneAsync(sceneName));
    }

    private IEnumerator LoadClientSceneAsync(string sceneName)
    {
        Show(); // show UI first

        yield return null; // ✅ wait 1 frame so UI renders

        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);

        while (!op.isDone)
        {
            float progress = Mathf.Clamp01(op.progress / 0.9f);
            // UpdateProgress(progress);
            // print(progress);
            yield return null;
        }

        Hide();
    }

}
