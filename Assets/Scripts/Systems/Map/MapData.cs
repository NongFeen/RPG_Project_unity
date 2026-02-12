using UnityEngine;

[CreateAssetMenu(menuName = "Maps/Map Data")]
public class MapData : ScriptableObject
{
    public MapName mapName;
    public string displayName;
    public string sceneName;
    public Sprite previewImage;
    public int recommendedLevel;
    public MapItemDrop mapItemDrop;
}
