using Unity.VisualScripting;
using UnityEngine;
[CreateAssetMenu(menuName = "Scriptable Objects/Item")]
public abstract class Item : ScriptableObject
{
    public int id;
    public string itemName;
    public string description;
    public Sprite image;
    public GameObject itemPrefab;
    public ItemType itemType;
    public abstract ItemInstance CreateInstance();
}
public enum ItemType
{
    Material,
    Weapon,
    Accessory
}

