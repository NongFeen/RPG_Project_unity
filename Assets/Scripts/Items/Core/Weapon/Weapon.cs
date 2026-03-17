using UnityEngine;

[CreateAssetMenu(fileName = "Weapon", menuName = "Scriptable Objects/Weapon")]
public class Weapon : Item
{
    [Header("Basic Information")]
    [SerializeField] public GameObject clientProjectilePrefab;
    [SerializeField] public GameObject serverProjectilePrefab;
    [SerializeField] public float baseDamage;
    [SerializeField] public float baseFirerate;
    [SerializeField] public int baseMaxammo;
    [SerializeField] public float baseReloadSpeed = 1;
    [SerializeField] public GameObject weaponPrefab;
    [Header("Random Stats Range on Drop (1.0 => 100%)")]
    [SerializeField] public RandomStatsRange randomStatsRange;
    [SerializeField] public WeaponStat weaponExtraStat;
    public override ItemInstance CreateInstance()
    {
        return ItemInstance.CreateInstance(this);
    }
    public override string ToString()
    {
        return $"{itemName} server{serverProjectilePrefab}";
    }
}
