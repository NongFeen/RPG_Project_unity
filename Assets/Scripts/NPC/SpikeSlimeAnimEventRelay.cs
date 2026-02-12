using UnityEngine;

public class SpikeSlimeAnimEventRelay : MonoBehaviour
{
    [SerializeField]private SlimeEnemy slime;

    private void Awake()
    {
        slime = GetComponentInParent<SlimeEnemy>();
    }

    // Called by Animation Event
    public void OnJumpAnimationEnd()
    {
        if (slime != null)
        {
            slime.OnJumpAnimationEnd();
        }
    }
}
