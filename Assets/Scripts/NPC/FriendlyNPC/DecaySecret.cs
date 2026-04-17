using UnityEngine;

public class DecaySecret : MonoBehaviour
{
    public void ConsumeSecret()
    {
        Destroy(gameObject);
    }
}
