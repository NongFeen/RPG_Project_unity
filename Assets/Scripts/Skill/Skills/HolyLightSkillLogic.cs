using UnityEngine;

public class HolyLightSkillLogic : SkillLogic
{
    public HolyLightSkillLogic(PlayerSkillController controller, SkillDefinition def): base(controller, def) { }

    public override bool Activate(Vector3 targetPos)
    {
        float radius = 0.8f; 
        Collider2D[] hits = Physics2D.OverlapCircleAll(targetPos, radius);

        GameObject closestEnemy = null;
        float closestDist = Mathf.Infinity;

        foreach (var hit in hits)
        {
            if (!hit.CompareTag("Enemy")) continue;

            float dist = Vector2.Distance(targetPos, hit.transform.position);
            if (dist < closestDist)
            {
                closestDist = dist;
                closestEnemy = hit.gameObject;
            }
        }

        if (closestEnemy == null)
            return false;

        var netObj = closestEnemy.GetComponent<Unity.Netcode.NetworkObject>();
        if (netObj == null)
            return false;
        
        controller.SpawnProjectileServerRpc(
            definition.magicNumber1,
            netObj.transform.position,
            Vector2.zero,
            definition.skillType
        );

        return true;
    }
}