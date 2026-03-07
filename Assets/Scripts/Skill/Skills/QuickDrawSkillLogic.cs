using UnityEngine;

public class QuickDrawSkillLogic : SkillLogic
{
    public QuickDrawSkillLogic(PlayerSkillController controller, SkillDefinition def): base(controller, def) { }
    readonly float spreadAngle = 30f; //max angle
    public override bool Activate(Vector3 targetPos)
    {
        Vector2 baseDir = (targetPos - controller.transform.position).normalized;

    int pelletCount = (int)definition.magicNumber2;
    float randomJitter = (int)definition.magicNumber3;        // randomness per pellet

    for (int i = 0; i < pelletCount; i++)
    {
        float t = (pelletCount == 1) ? 0.5f : (float)i / (pelletCount - 1);

        // perfect spread
        float angle = Mathf.Lerp(-spreadAngle / 2f, spreadAngle / 2f, t);

        // add randomness
        angle += Random.Range(-randomJitter, randomJitter);

        Vector2 dir = Quaternion.Euler(0, 0, angle) * baseDir;

        controller.SpawnProjectileServerRpc(
            definition.magicNumber1,
            controller.transform.position,
            dir,
            definition.skillType
        );
    }
        return true;
    }
}