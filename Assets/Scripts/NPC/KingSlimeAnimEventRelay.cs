using UnityEngine;

public class KingSlimeAnimEventRelay : MonoBehaviour
{
    [SerializeField]private KingSlime_NPC slime;

    private void Awake()
    {
        slime = GetComponentInParent<KingSlime_NPC>();
    }

    // Called by Animation Event
    public void OnSlamAnimationEnd()
    {
        if (slime != null)
        {
            slime.OnSlamEnd();
        }
    }
}
