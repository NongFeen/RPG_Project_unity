using UnityEngine;
using UnityEngine.UI;

public class MapInvoker : MonoBehaviour
{
    [SerializeReference] private MapName mapName;
    private Button _button;

    private void Awake()
    {
        _button = GetComponent<Button>();
    }

    private void OnEnable()
    {
        RefreshLockState();
    }

    public void RefreshLockState()
    {
        bool unlocked = GameManager.Instance != null && GameManager.Instance.IsMapUnlocked(mapName);

        if (_button != null)
            _button.interactable = unlocked;
    }

    public void SelectMap(MapInvoker curMapName)
    {
        if (GameManager.Instance != null && !GameManager.Instance.IsMapUnlocked(curMapName.mapName))
            return;

        GameManager.Instance.SelectMap(curMapName.mapName);
    }
}
