using UnityEngine;

public class AlongSkillLogic : SkillLogic
{
    public AlongSkillLogic(PlayerSkillController controller, SkillDefinition def): base(controller, def) { }

    public override bool Activate(Vector3 targetPos)
    {
        // Buff the player
        PlayerStats playerStats = controller.GetComponent<PlayerStats>();
        playerStats.AddBuffServer(BuffType.Along, definition.magicNumber2);
        
        // Buff all active AriseProjectiles owned by this player
        BuffAllSummons(playerStats);
        
        return true;
    }

    private void BuffAllSummons(PlayerStats playerStats)
    {
        Player ownerPlayer = playerStats.GetComponent<Player>();
        if (ownerPlayer == null) return;

        // Find all AriseProjectiles in the scene
        AriseProjectile[] allProjectiles = Object.FindObjectsByType<AriseProjectile>(FindObjectsSortMode.None);
        
        foreach (AriseProjectile projectile in allProjectiles)
        {
            // Only buff projectiles owned by this player
            if (projectile.owner == ownerPlayer)
            {
                // Apply damage boost to the projectile
                projectile.damage *= (1f + definition.magicNumber1);
            }
        }
    }
}
