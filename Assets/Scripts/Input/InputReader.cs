using System;
using System.Collections.Generic;
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
    public enum GameDevice
    {
        KeyboardAndMouse,
        GamePad
    }
    public GameDevice activeGameDevice;
    public Action<bool> OnGameDeviceChange;
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
        InputSystem.onActionChange += InputAction_OnActionChange;
    }
    private void InputAction_OnActionChange(object arg1, InputActionChange inputActionChange)
    {
        if(inputActionChange == InputActionChange.ActionPerformed && arg1 is InputAction)
        {
            InputAction inputAction = arg1 as InputAction;
            if (inputAction.activeControl == null)
            {
                return;
            }
            if(inputAction.activeControl.device.displayName == "VirtualMouse")
            {
                //ignore virtual mouse
                return;
            }
            if(inputAction.activeControl.device is Gamepad)
            {
                if(activeGameDevice != GameDevice.GamePad)
                    ChangeActiveGameDevice(GameDevice.GamePad);
            }
            else
            {
                if(activeGameDevice != GameDevice.KeyboardAndMouse)
                    ChangeActiveGameDevice(GameDevice.KeyboardAndMouse);
            }
        }
    }
    private void ChangeActiveGameDevice(GameDevice activeGameDevice)
    {
        this.activeGameDevice = activeGameDevice;
        Debug.Log("New Device Detected" + activeGameDevice);    

        //disable cursor when use controller
        Cursor.visible = activeGameDevice == GameDevice.KeyboardAndMouse;
        //return true if use MnK
        OnGameDeviceChange?.Invoke(activeGameDevice == GameDevice.KeyboardAndMouse);
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
        if (controls == null)
        {
            Debug.LogError("Controls not initialized");
            return;
        }
        var action = controls.asset.FindAction(actionName);
        if (action == null)
        {
            Debug.LogError($"Action '{actionName}' not found");
            return;
        }
        if (bindingIndex < 0 || bindingIndex >= action.bindings.Count)
        {
            Debug.LogError($"Invalid binding index {bindingIndex} for action '{actionName}'");
            return;
        }
        if (currentRebind != null)
        {
            currentRebind.Cancel();
            currentRebind.Dispose();
            currentRebind = null;
        }
        controls.Player.Disable();
        OnRebindStarted?.Invoke(actionName);
        var binding = action.bindings[bindingIndex];
        if (binding.isComposite)
        {
            var firstPartIndex = bindingIndex + 1;
            if (firstPartIndex >= action.bindings.Count || !action.bindings[firstPartIndex].isPartOfComposite)
            {
                Debug.LogError($"Composite binding '{actionName}' has no parts to rebind.");
                controls.Player.Enable();
                return;
            }
            StartRebindOperation(action, firstPartIndex, true, actionName);
        }
        else
        {
            StartRebindOperation(action, bindingIndex, false, actionName);
        }
    }
    public string GetBindingName(string actionName, int bindingIndex = 0)
    {
        var action = controls.asset.FindAction(actionName);
        if (action == null) return string.Empty;
        if (bindingIndex < 0 || bindingIndex >= action.bindings.Count) return string.Empty;
        var binding = action.bindings[bindingIndex];
        if (binding.isComposite)
        {
            var parts = new List<string>();
            for (int i = bindingIndex + 1; i < action.bindings.Count; i++)
            {
                if (!action.bindings[i].isPartOfComposite)
                    break;
                parts.Add(action.GetBindingDisplayString(i));
            }
            return string.Join(" / ", parts);
        }
        return action.GetBindingDisplayString(bindingIndex);
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
    
    private void StartRebindOperation(InputAction action, int bindingIndex, bool isCompositeRebind, string actionName)
    {
        var rebind = action.PerformInteractiveRebinding(bindingIndex)
            .WithCancelingThrough("<Keyboard>/escape")
            .WithControlsExcluding("<Touchscreen>");

        // Allow mouse buttons for PrimaryFire. For other actions, exclude pointer input.
        if (!string.Equals(actionName, "PrimaryFire", StringComparison.Ordinal))
        {
            rebind.WithControlsExcluding("<Pointer>");
        }

        currentRebind = rebind
            .OnComplete(operation =>
            {
                operation.Dispose();
                currentRebind = null;
                if (isCompositeRebind)
                {
                    var nextIndex = bindingIndex + 1;
                    if (nextIndex < action.bindings.Count && action.bindings[nextIndex].isPartOfComposite)
                    {
                        StartRebindOperation(action, nextIndex, true, actionName);
                        return;
                    }
                }
                controls.Player.Enable();
                SaveRebinds();
                OnBindingsChanged?.Invoke();
                Debug.Log($"Rebind complete: {actionName}");
            })
            .OnCancel(operation =>
            {
                operation.Dispose();
                currentRebind = null;
                controls.Player.Enable();
                Debug.Log("Rebind canceled");
            });
        currentRebind.Start();
    }
#endregion
}
