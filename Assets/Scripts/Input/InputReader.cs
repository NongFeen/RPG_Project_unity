using System;
using UnityEngine;
using UnityEngine.InputSystem;
using static Controls;

[CreateAssetMenu(fileName = "New Input Reader", menuName = "Input/Input Reader")]
public class InputReader : ScriptableObject, IPlayerActions
{
    // === Public Events for Game Code ===
    public event Action<bool> PrimaryFireEvents;
    public event Action<bool> ReloadWeaponEvents;
    public event Action<bool> OpenInventoryEvents;
    public event Action<int> SelectActiveItemEvents;
    public event Action<int> SkillUseEvents;
    public event Action<Vector2> MoveEvents;
    public event Action<bool> EscapeKey;
    public event Action OnBindingsChanged;
    public event Action<string> OnRebindStarted;
    private InputActionRebindingExtensions.RebindingOperation currentRebind;
    public InputAction OpenInventoryAction => controls.Player.OpenInventory;
    public Vector2 AimPosition { get; private set; }

    private Controls controls;

    // === Unity Lifecycle ===
    private void OnEnable()
    {
        Init();
    }

    private void OnDisable()
    {
        if (controls != null)
        {
            controls.Player.Disable();
            controls.Player.RemoveCallbacks(this);
        }
    }

    // === Init / Re-Init ===
    public void Init()
    {
        if (controls != null)
        {
            // Reset callbacks when reloading scene
            controls.Player.Disable();
            controls.Player.RemoveCallbacks(this);
        }
        else
        {
            controls = new Controls();
        }
        LoadRebinds();
        controls.Player.SetCallbacks(this);
        controls.Player.Enable();
    }

    // === Input Callbacks ===
    public void OnMove(InputAction.CallbackContext context)
    {
        MoveEvents?.Invoke(context.ReadValue<Vector2>());
    }

    public void OnPrimaryFire(InputAction.CallbackContext context)
    {
        if (context.performed)
            PrimaryFireEvents?.Invoke(true);
        else if (context.canceled)
            PrimaryFireEvents?.Invoke(false);
    }

    public void OnAim(InputAction.CallbackContext context)
    {
        AimPosition = context.ReadValue<Vector2>();
    }

    public void OnOpenInventory(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            // Debug.Log("InputReader: OpenInventory performed");
            OpenInventoryEvents?.Invoke(true);
        }
        else if (context.canceled)
        {
            OpenInventoryEvents?.Invoke(false);
        }
    }

    public void OnOnSelectSlot1(InputAction.CallbackContext context)
    {
        if (context.performed) SelectActiveItemEvents?.Invoke(0);
    }

    public void OnOnSelectSlot2(InputAction.CallbackContext context)
    {
        if (context.performed) SelectActiveItemEvents?.Invoke(1);
    }

    public void OnOnSelectSlot3(InputAction.CallbackContext context)
    {
        if (context.performed) SelectActiveItemEvents?.Invoke(2);
    }

    public void OnReload(InputAction.CallbackContext context)
    {
        if (context.performed)
            ReloadWeaponEvents?.Invoke(true);
        else if (context.canceled)
            ReloadWeaponEvents?.Invoke(false);
    }
    //V Q F
    //1 2 3
    public void OnSkill1(InputAction.CallbackContext context)
    {
       if (context.performed) SkillUseEvents?.Invoke(0);
    }
    public void OnSkill2(InputAction.CallbackContext context)
    {
        if (context.performed) SkillUseEvents?.Invoke(1);
    }

    public void OnSkill3(InputAction.CallbackContext context)
    {
        if (context.performed) SkillUseEvents?.Invoke(2);
    }

    public void OnEscape(InputAction.CallbackContext context)
    {
        if(context.performed) EscapeKey?.Invoke(true);
        else if (context.canceled) EscapeKey?.Invoke(false); 
    } 

    #region Rebinding 
    public void StartRebind(string actionName, int bindingIndex = 0)
    {
        // 🔒 Safety: controls must exist
        if (controls == null)
        {
            Debug.LogError("Controls not initialized");
            return;
        }

        var action = controls.asset.FindAction(actionName);

        // 🔒 Safety: action must exist
        if (action == null)
        {
            Debug.LogError($"Action '{actionName}' not found");
            return;
        }

        // 🔒 Safety: binding index valid
        if (bindingIndex < 0 || bindingIndex >= action.bindings.Count)
        {
            Debug.LogError($"Invalid binding index {bindingIndex} for action '{actionName}'");
            return;
        }

        // 🧹 Cancel previous rebind if exists
        if (currentRebind != null)
        {
            currentRebind.Cancel();
            currentRebind.Dispose();
            currentRebind = null;
        }

        // 🔕 Disable ALL input during rebind (important)
        controls.Player.Disable();

        // 📢 Notify UI (e.g. "Press any key...")
        OnRebindStarted?.Invoke(actionName);

        // 🎯 Start rebinding
        currentRebind = action.PerformInteractiveRebinding(bindingIndex)
            .WithCancelingThrough("<Keyboard>/escape")
            .WithControlsExcluding("Mouse")
            .WithControlsExcluding("<Pointer>")
            .WithControlsExcluding("<Touchscreen>")

            .OnComplete(operation =>
            {
                operation.Dispose();
                currentRebind = null;

                // 🔊 Re-enable input
                controls.Player.Enable();

                // 💾 Save binding
                SaveRebinds();

                // 🔄 Notify UI to refresh
                OnBindingsChanged?.Invoke();

                Debug.Log($"Rebind complete: {actionName}");
            })

            .OnCancel(operation =>
            {
                operation.Dispose();
                currentRebind = null;

                // 🔊 Re-enable input
                controls.Player.Enable();

                Debug.Log("Rebind canceled");
            });

        currentRebind.Start();
    }
    public string GetBindingName(string actionName, int bindingIndex = 0)
    {
        var action = controls.asset.FindAction(actionName);
        return action.bindings[bindingIndex].ToDisplayString();
    }
    public void SaveRebinds()
    {
        var rebinds = controls.asset.SaveBindingOverridesAsJson();
        PlayerPrefs.SetString("rebinds", rebinds);
    }
    public void LoadRebinds()
    {
        if (PlayerPrefs.HasKey("rebinds"))
        {
            var rebinds = PlayerPrefs.GetString("rebinds");
            controls.asset.LoadBindingOverridesFromJson(rebinds);
        }
    }
    public void ResetBindings()
    {
        controls.asset.RemoveAllBindingOverrides();
        SaveRebinds();
        OnBindingsChanged?.Invoke();
    }
    #endregion
}
