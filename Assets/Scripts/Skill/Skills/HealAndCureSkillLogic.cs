using UnityEngine;

public class WellBlessing : SkillLogic
{
    public WellBlessing(PlayerSkillController controller, SkillDefinition def): base(controller, def) { }

    public override bool Activate(Vector3 targetPos)
    {
        float radius = 0.8f; 
        Collider2D[] hits = Physics2D.OverlapCircleAll(targetPos, radius);

        GameObject closestPlayer = null;
        float closestDist = Mathf.Infinity;

        foreach (var hit in hits)
        {
            if (!hit.CompareTag("Player")) continue;

            float dist = Vector2.Distance(targetPos, hit.transform.position);
            if (dist < closestDist)
            {
                closestDist = dist;
                closestPlayer = hit.gameObject;
            }
        }

        if (closestPlayer == null)
            return false;

        closestPlayer.TryGetComponent<PlayerStats>(out var player);
        if (player == null)
            return false;
        player.HealServerRpc(definition.magicNumber1);
        return true;
    }
}