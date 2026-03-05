using UnityEngine;
using UnityEngine.UI;

public class SkillSlotUI : MonoBehaviour
{
    [SerializeField] private Image skillIcon;
    [SerializeField] private Slider slider;

    [SerializeField] private SkillLogic skill;
    [SerializeField] private float maxCooldown;

    public void Bind(SkillLogic skillLogic, SkillDefinition skillDefinition)
    {
        skill = skillLogic;
        maxCooldown = skillDefinition.cooldown;
        skillIcon.sprite = skillDefinition.icon;
    }

    void Update()
    {
        if (skill == null) return;
        float remain = skill.CooldownRemaining();
        slider.value = remain/maxCooldown;
    }
}