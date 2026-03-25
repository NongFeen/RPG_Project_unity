using UnityEngine;

public class PlayerSkillUI : MonoBehaviour, IPlayerStatUI
{
    public PlayerSkillController playerSkillController;

    [SerializeField] private SkillSlotUI skillVSlot;
    [SerializeField] private SkillSlotUI skillQSlot;
    [SerializeField] private SkillSlotUI skillFSlot;

    public void SetPlayerData(GameObject player)
    {
        playerSkillController = player.GetComponent<PlayerSkillController>();
        
        if(playerSkillController.SkillV != null)
        {
            skillVSlot.Bind(
                playerSkillController.SkillV,
                playerSkillController.SkillV.GetSkillDefinition(),
                "Skill1"
            );
        }
        if(playerSkillController.SkillQ != null)
        {
            skillQSlot.Bind(
                playerSkillController.SkillQ,
                playerSkillController.SkillQ.GetSkillDefinition(),
                "Skill2"
            );
        }
        if(playerSkillController.SkillF != null)
        {
            skillFSlot.Bind(
                playerSkillController.SkillF,
                playerSkillController.SkillF.GetSkillDefinition(),
                "Skill3"
            );
        }
    }
}
