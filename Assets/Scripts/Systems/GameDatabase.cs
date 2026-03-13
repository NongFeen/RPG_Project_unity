using UnityEngine;

public class GameDatabase : MonoBehaviour
{
    public static GameDatabase Instance { get; private set; }
    [SerializeField] private ItemDatabase itemData;
    [SerializeField] private ClassDataBase classData;
    [SerializeField] private MapDatabase mapData;
    [SerializeField] private BuffDatabase buffData;
    [SerializeField] private ExperienceData experienceData;

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
    public BuffDatabase GetBuffDatabase()
    {
        return buffData;
    }
    public ExperienceData GetExperienceData()
    {
        return experienceData;
    }
}
