using UnityEngine;

public interface IWeapon
{
    void OnShoot(Vector2 Direction);
    void OnReload();
    void OnDrawWeapon();
    void OnStowWeapon();
    void OnSpecialReload();
    void OnSpecialShoot();
}
