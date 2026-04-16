using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillSlotUI : MonoBehaviour
{
    [SerializeField] private Image skillIcon;
    [SerializeField] private Slider slider;

    [SerializeField] private SkillLogic skill;
    [SerializeField] private float maxCooldown;
    [SerializeField] private TextMeshProUGUI skillKeytText;
    [SerializeField] private InputReader inputReader;
    [SerializeField] private string skillActionName;
    [SerializeField] private int keyboardBindingIndex = 0;
    [SerializeField] private int gamepadBindingIndex = 1;

    public void Bind(SkillLogic skillLogic, SkillDefinition skillDefinition, string actionName)
    {
        skill = skillLogic;
        maxCooldown = skillDefinition.cooldown;
        skillIcon.sprite = skillDefinition.icon;
        skillActionName = actionName;
        UpdateKeyText();
    }

    private void OnEnable()
    {
        if (inputReader != null)
        {
            inputReader.OnBindingsChanged += HandleBindingsChanged;
            inputReader.OnGameDeviceChange += HandleDeviceChanged;
        }
        UpdateKeyText();
    }

    private void OnDisable()
    {
        if (inputReader != null)
        {
            inputReader.OnBindingsChanged -= HandleBindingsChanged;
            inputReader.OnGameDeviceChange -= HandleDeviceChanged;
            skillIcon.sprite = null;
        }
    }

    private void HandleBindingsChanged()
    {
        UpdateKeyText();
    }

    private void HandleDeviceChanged(bool _)
    {
        UpdateKeyText();
    }

    void Update()
    {
        if (skill == null) return;
        float remain = skill.CooldownRemaining();
        slider.value = remain/maxCooldown;
    }

    private void UpdateKeyText()
    {
        if (skillKeytText == null) return;
        if (inputReader == null || string.IsNullOrWhiteSpace(skillActionName))
        {
            skillKeytText.text = string.Empty;
            return;
        }

        int bindingIndex = inputReader.activeGameDevice == InputReader.GameDevice.GamePad
            ? gamepadBindingIndex
            : keyboardBindingIndex;

        string bindingName = inputReader.GetBindingName(skillActionName, bindingIndex);
        if (string.IsNullOrEmpty(bindingName) && bindingIndex != 0)
        {
            bindingName = inputReader.GetBindingName(skillActionName, 0);
        }
        skillKeytText.text = bindingName;
    }
}
