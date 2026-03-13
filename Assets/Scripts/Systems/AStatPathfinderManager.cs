using UnityEngine;
using Pathfinding;
public class xStatPathfinderManager : MonoBehaviour
{
    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }   
}
