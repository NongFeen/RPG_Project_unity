using UnityEngine;

public interface IWeapon
{
    void OnShoot(Transform firePoint,Vector2 Direction);
    void OnReload();
    void OnDrawWeapon();
    void OnStowWeapon();
    void OnSpecialReload();
    void OnSpecialShoot();
}
