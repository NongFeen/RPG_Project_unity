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

        skillVSlot.Bind(
            playerSkillController.SkillV,
            playerSkillController.SkillV.GetSkillDefinition()
        );

        skillQSlot.Bind(
            playerSkillController.SkillQ,
            playerSkillController.SkillQ.GetSkillDefinition()
        );

        skillFSlot.Bind(
            playerSkillController.SkillF,
            playerSkillController.SkillF.GetSkillDefinition()
        );
    }
}