using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingScreenManager : MonoBehaviour
{
    public static LoadingScreenManager Instance;

    [Serializable]
    private class LoadingTip
    {
        [TextArea(2, 5)] public string text;
        public Sprite image;
    }

    [SerializeField] private GameObject loadingCanvas;
    [SerializeField] private CanvasGroup loadingCanvasGroup;
    [SerializeField] private TextMeshProUGUI loadingText;
    [SerializeField] private TextMeshProUGUI tipsText;
    [SerializeField] private Image tipsImage;
    [SerializeField] private List<LoadingTip> loadingTips = new();
    [SerializeField] private float fadeInDuration = 0.15f;
    [SerializeField] private float fadeOutDuration = 0.2f;


    private NetworkSceneManager NetworkSceneManager => NetworkManager.Singleton.SceneManager;

    private Coroutine delayedShowRoutine;
    private Coroutine fadeRoutine;
    private bool isLoading;
    private float delayTime = 5;
    private bool isBinded = false;
    private int lastTipIndex = -1;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        // DontDestroyOnLoad(gameObject);
        EnsureCanvasGroup();
        HideImmediate();
    }
    private void OnEnable()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
            // In case this object is enabled after we're already connected.
            if (NetworkManager.Singleton.IsClient)
            {
                BindSceneEvents();
            }
        }
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
        }
        if (NetworkManager.Singleton == null) return;
        NetworkSceneManager.OnSceneEvent -= HandleOnSceneEvent;
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!isLoading) return;
        isLoading = false;
        Hide();
    }
    private void OnClientConnected(ulong clientId)
    {
        if (NetworkManager.Singleton == null) return;
        if (clientId != NetworkManager.Singleton.LocalClientId) return;
        BindSceneEvents();
    }
    private void OnClientDisconnected(ulong clientId)
    {
        if (NetworkManager.Singleton == null) return;
        if (clientId != NetworkManager.Singleton.LocalClientId) return;
        NetworkSceneManager.OnSceneEvent -= HandleOnSceneEvent;
        isBinded = false;
    }
    public void BindSceneEvents()
    {
        if (NetworkManager.Singleton == null )
        {
            Debug.LogWarning("NetworkManager is null, cannot bind scene events");
            return;
        }
        if(isBinded) return;

        NetworkManager.Singleton.SceneManager.OnSceneEvent += HandleOnSceneEvent;
        isBinded = true;
        Debug.Log("subscribed to SceneManager events");
    }

    private void HandleOnSceneEvent(SceneEvent sceneEvent)
    {
        if(NetworkManager.Singleton.LocalClientId != sceneEvent.ClientId)
            return;
        if(sceneEvent.SceneEventType == SceneEventType.Load)
        {
            isLoading = true;
            Show();
            StartCoroutine(TrackProgress(sceneEvent.AsyncOperation));
        }
        if(sceneEvent.SceneEventType == SceneEventType.LoadComplete)
        {
            isLoading = false;
            Hide();
        }
    }
    private IEnumerator TrackProgress(AsyncOperation op)
    {
        if (loadingText == null)
        {
            yield break;
        }
        if (op == null)
        {
            loadingText.text = "Loading...";
            yield break;
        }

        while (!op.isDone)
        {
            float progress = Mathf.Clamp01(op.progress / 0.9f);
            int pct = Mathf.FloorToInt(progress * 100f);
            if (pct >= 100) pct = 99;
            loadingText.text = $"Loading... {pct}%";
            yield return null; 
        }
        loadingText.text = "Loading... 100%";
    }

    private IEnumerator DelayedShow()
    {
        yield return new WaitForSeconds(delayTime);

        if (isLoading)
            Show();
    }

    private void Show()
    {
        EnsureCanvasGroup();
        if (fadeRoutine != null) StopCoroutine(fadeRoutine);
        loadingCanvas.SetActive(true);
        if (loadingText != null)
        {
            loadingText.text = "Loading... 0%";
        }
        SetRandomTip();
        loadingCanvasGroup.alpha = 0f;
        fadeRoutine = StartCoroutine(FadeCanvas(1f, fadeInDuration));
    }

    private void Hide()
    {
        EnsureCanvasGroup();
        if (fadeRoutine != null) StopCoroutine(fadeRoutine);
        fadeRoutine = StartCoroutine(FadeOutAndDisable());
    }

    private void HideImmediate()
    {
        if (loadingCanvas == null) return;
        loadingCanvas.SetActive(false);
        if (loadingCanvasGroup != null)
        {
            loadingCanvasGroup.alpha = 0f;
            loadingCanvasGroup.blocksRaycasts = false;
            loadingCanvasGroup.interactable = false;
        }
    }
    private void EnsureCanvasGroup()
    {
        if (loadingCanvasGroup != null || loadingCanvas == null) return;
        loadingCanvasGroup = loadingCanvas.GetComponent<CanvasGroup>();
        if (loadingCanvasGroup == null)
        {
            loadingCanvasGroup = loadingCanvas.AddComponent<CanvasGroup>();
        }
    }

    private IEnumerator FadeOutAndDisable()
    {
        yield return FadeCanvas(0f, fadeOutDuration);
        if (loadingCanvas != null)
        {
            loadingCanvas.SetActive(false);
        }
    }

    private IEnumerator FadeCanvas(float targetAlpha, float duration)
    {
        if (loadingCanvasGroup == null)
            yield break;

        float startAlpha = loadingCanvasGroup.alpha;
        float t = 0f;
        loadingCanvasGroup.blocksRaycasts = true;
        loadingCanvasGroup.interactable = false;

        if (duration <= 0f)
        {
            loadingCanvasGroup.alpha = targetAlpha;
            loadingCanvasGroup.blocksRaycasts = targetAlpha > 0.01f;
            loadingCanvasGroup.interactable = false;
            yield break;
        }

        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float lerp = Mathf.Clamp01(t / duration);
            loadingCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, lerp);
            yield return null;
        }

        loadingCanvasGroup.alpha = targetAlpha;
        bool visible = targetAlpha > 0.01f;
        loadingCanvasGroup.blocksRaycasts = visible;
        loadingCanvasGroup.interactable = false;
    }

    private void SetRandomTip()
    {
        if (tipsText != null) tipsText.text = string.Empty;
        if (tipsImage != null)
        {
            tipsImage.sprite = null;
            tipsImage.enabled = false;
        }

        if (loadingTips == null || loadingTips.Count == 0)
            return;

        int index = UnityEngine.Random.Range(0, loadingTips.Count);
        if (loadingTips.Count > 1 && index == lastTipIndex)
        {
            index = (index + 1) % loadingTips.Count;
        }
        lastTipIndex = index;

        LoadingTip tip = loadingTips[index];

        if (tipsText != null)
            tipsText.text = tip != null ? tip.text : string.Empty;

        if (tipsImage != null)
        {
            tipsImage.sprite = tip != null ? tip.image : null;
            tipsImage.enabled = tipsImage.sprite != null;
        }
    }

    public void ShowLoading()
    {
        BindSceneEvents();
        isLoading = true;
        Show();
    }

    public void LoadScene(string sceneName)
    {
        BindSceneEvents();
        if (UIManager.Instance != null)
        {
            //close all menu before it's unloaded
            UIManager.Instance.CloseAllMenus();
        }
        isLoading = true;
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
        if (UIManager.Instance != null)
        {
            UIManager.Instance.CloseAllMenus();
        }
        isLoading = true;
        StartCoroutine(LoadClientSceneAsync(sceneName));
    }

    private IEnumerator LoadClientSceneAsync(string sceneName)
    {
        Show(); 
        yield return null; 
        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);

        while (!op.isDone)
        {
            float progress = Mathf.Clamp01(op.progress / 0.9f);
            if (loadingText != null)
            {
                int pct = Mathf.FloorToInt(progress * 100f);
                if (pct >= 100) pct = 99;
                loadingText.text = $"Loading... {pct}%";
            }
            yield return null;
        }
        if (loadingText != null)
        {
            loadingText.text = "Loading... 100%";
        }

        Hide();
    }

}
