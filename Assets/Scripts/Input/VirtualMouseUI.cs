using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

[RequireComponent(typeof(VirtualMouseInput))]
public class VirtualMouseUI : MonoBehaviour
{
    [SerializeField]private VirtualMouseInput virtualMouseInput;
    [SerializeField]private RectTransform canvasRectTransform;
    [SerializeField] InputReader inputReader;
    [Header("Virtual Mouse Stick Actions")]
    [SerializeField] private InputActionProperty menuStickAction;
    [SerializeField] private InputActionProperty gameplayStickAction;
    private Graphic cursorGraphic;
    private bool pendingIsMnK;
    private bool hasPendingChange;
    private bool hasLastGameState;
    private GameState lastGameState;
    private InputActionProperty currentStickAction;
    private void Awake()
    {
        virtualMouseInput = GetComponent<VirtualMouseInput>();
        if (virtualMouseInput != null)
        {
            cursorGraphic = virtualMouseInput.cursorGraphic;
        }

    }
    private void Start()
    {
        inputReader.OnGameDeviceChange += DeviceChange;
        ApplyStickAction(GameManager.Instance != null ? GameManager.Instance.gameState : GameState.InGame);
    }

    private void DeviceChange(bool isMnK)
    {
        pendingIsMnK = isMnK;
        hasPendingChange = true;
    }

    private void UpdateVisibiliy(bool isMnK)
    {
        if (isMnK)
            Hide();
        else
            Show();
    }
    void Hide()
    {
        if (cursorGraphic != null)
        {
            cursorGraphic.enabled = false;
        }
    }
    void Show()
    {
        if (cursorGraphic != null)
        {
            cursorGraphic.enabled = true;
        }
    }

    void Update()
    {
        if (hasPendingChange)
        {
            hasPendingChange = false;
            UpdateVisibiliy(pendingIsMnK);
        }

        if (GameManager.Instance != null)
        {
            var state = GameManager.Instance.gameState;
            if (!hasLastGameState || state != lastGameState)
            {
                hasLastGameState = true;
                lastGameState = state;
                ApplyStickAction(state);
            }
        }

        transform.localScale = Vector3.one * (1f/ canvasRectTransform.localScale.x);
    }
    private void LateUpdate()
    {
        if (virtualMouseInput == null) return;
        if (virtualMouseInput.virtualMouse == null) return;
        if (!virtualMouseInput.virtualMouse.added) return;

        Vector2 virtualMousePosition = virtualMouseInput.virtualMouse.position.value;
        virtualMousePosition.x = Mathf.Clamp(virtualMousePosition.x,0f,Screen.width);
        virtualMousePosition.y = Mathf.Clamp(virtualMousePosition.y,0f,Screen.height);
        InputState.Change(virtualMouseInput.virtualMouse.position,virtualMousePosition);
    }

    private void ApplyStickAction(GameState state)
    {
        bool useMenuStick = state == GameState.MainMenu || state == GameState.Lobby;
        var target = useMenuStick ? menuStickAction : gameplayStickAction;

        if (SameAction(target, currentStickAction))
        {
            return;
        }

        if (currentStickAction.action != null)
        {
            currentStickAction.action.Disable();
        }

        currentStickAction = target;
        if (virtualMouseInput != null)
        {
            virtualMouseInput.stickAction = currentStickAction;
        }
        currentStickAction.action?.Enable();
    }

    private static bool SameAction(InputActionProperty a, InputActionProperty b)
    {
        return a.reference == b.reference && a.action == b.action;
    }
}
