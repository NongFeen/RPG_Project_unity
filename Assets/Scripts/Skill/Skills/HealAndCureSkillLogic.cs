using System.Collections.Generic;
using UnityEngine;

public class HealAndCure : SkillLogic
{
    public HealAndCure(PlayerSkillController controller, SkillDefinition def): base(controller, def) { }

    public override bool Activate(Vector3 targetPos)
    {
        float radius = 0.8f; 
        Collider2D[] hits = Physics2D.OverlapCircleAll(targetPos, radius);

        GameObject closestPlayer = null;
        float closestDist = Mathf.Infinity;

        foreach (var hit in hits)
        {
            //priset change class to necro
            if (hit.CompareTag("DecaySecret"))
            {
                Debug.Log("Found DecaySecret, Changing class.");
                controller.GetComponent<PlayerStats>().ChangeClassServerRpc(ClassType.Necro);
                hit.GetComponent<DecaySecret>().ConsumeSecret();
            }
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
        
        List<BuffType> toRemove = new List<BuffType>();
        foreach (BaseBuff buff in player.GetActiveBuffs().Values)
        {
            if (buff.isDebuff)
            {
                Debug.Log("Removing debuff: " + buff.data.buffType);
                toRemove.Add(buff.data.buffType);
            }
        }

        foreach (BuffType type in toRemove)
        {
            player.RemoveBuffServer(type);
        }
        return true;
    }
}