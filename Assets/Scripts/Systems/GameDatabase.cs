using UnityEngine;

public class GameDatabase : MonoBehaviour
{
    public static GameDatabase Instance { get; private set; }
    [SerializeField] ItemDatabase itemData;
    [SerializeField] ClassDataBase classData;
    [SerializeField] MapDatabase mapData;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    public ItemDatabase GetItemDatabase()
    {
        return itemData;
    }
    public ClassDataBase GetClassDatabase()
    {
        return classData;
    }
    public MapDatabase GetMapDatabase()
    {
        return mapData;
    }
}
