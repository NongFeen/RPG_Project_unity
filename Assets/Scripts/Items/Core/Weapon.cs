using UnityEngine;

[CreateAssetMenu(fileName = "Weapon", menuName = "Scriptable Objects/Weapon")]
public class Weapon : Item
{
    [SerializeField] public GameObject clientProjectilePrefab;
    [SerializeField] public GameObject serverProjectilePrefab;
    [SerializeField] public float baseDamage;
    [SerializeField] public float baseFirerate;
    [SerializeField] public int baseMaxammo;
    [SerializeField] public float baseReloadSpeed = 1;
    [SerializeField] public Animator animator;
    [SerializeField] public AnimationClip animationClip;
    [SerializeField] public GameObject weaponPrefab;

    [SerializeField]public WeaponStat weaponExtraStat;

    public override ItemInstance CreateInstance()
    {
        return ItemInstance.CreateInstance(this);

    }
    public override string ToString()
    {
        return $"{itemName} server{serverProjectilePrefab}";
    }
}
