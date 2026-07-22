using TMPro;
using UnityEngine;

public class SimpleCountBulletUI : MonoBehaviour
{
    public TextMeshProUGUI text;
    public void Awake()
    {
        if(GameplayLogger.Instance == null)
        {
            // Debug.LogError("GameplayLogger instance not found! Please ensure a GameplayLogger is present in the scene.");
            this.enabled = false; 
            text.gameObject.SetActive(false); 
        }
    }
    void Update()
    {
        int count = NetworkObjectPool.Singleton != null
                ? NetworkObjectPool.Singleton.ActiveObjectsCount
                : 0;
        text.text = $"Objects: {count}";
    }
}
