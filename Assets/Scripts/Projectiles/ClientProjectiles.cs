using Unity.VisualScripting;
using UnityEngine;

public class ClientProjectiles : MonoBehaviour
{
    // Update is called once per frame

    [SerializeField] private float lifeTime = 3f;
    // [SerializeField] private int pierce = 1;
    // [SerializeField] private bool canHitPlayer = false;
    private Vector2 direction;
    // private void Initialize(Vector2 dir) //for spawn
    // {
    //     direction = dir.normalized;
    //     Destroy(gameObject, lifeTime);//destroy this object after LifeTime
    // }
    private void Start()
    {
        // direction = dir.normalized;
        Destroy(gameObject, lifeTime);//destroy this object after LifeTime
    }
    // private void OnTriggerEnter2D(Collider2D collision)
    // {
    //     if (collision.CompareTag("Player"))
    //     {
    //         if (!canHitPlayer) return;

    //         pierce -= 1;
    //         if (pierce < 1)
    //             Destroy(gameObject);
    //     }
    // }
}
