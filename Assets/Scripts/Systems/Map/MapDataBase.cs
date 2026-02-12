using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "MapDatabase", menuName = "System/Map Database")]
public class MapDatabase : ScriptableObject
{

    [SerializeField] private List<MapData> maps;

    private Dictionary<MapName, MapData> mapDict;

    private void OnEnable()
    {
        BuildDictionary();
    }
    private void BuildDictionary()
    {
        mapDict = new Dictionary<MapName, MapData>();

        foreach (var map in maps)
        {
            mapDict[map.mapName] = map;
        }
    }
    public MapData GetMapData(MapName name)
    {
        return mapDict[name];
    }
}
