using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class WeaponInventorySlot : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] public WeaponInstance StoredWeapon { get; private set; }
    [SerializeField] private Image weaponImage;

    public bool IsEmpty => StoredWeapon == null;

    private void Awake()
    {
        if (weaponImage != null)
            weaponImage.preserveAspect = true;
    }

    public void SetWeapon(WeaponInstance instance)
    {
        StoredWeapon = instance;
        ShowWeapon();
    }

    public void ShowWeapon()
    {
        // if (IsEmpty || StoredWeapon.IsEmpty) return;
        if (IsEmpty ||StoredWeapon.IsEmpty)
        {
            weaponImage.sprite=null;
            weaponImage.color = new Color(1,1,1,0);
            return;
        }
        else
        {
            weaponImage.sprite = StoredWeapon.weaponData.image;
            weaponImage.color = new Color(1,1,1,1);
        }
    }
    public void ClearSlot()
    {
        StoredWeapon = null;
        ShowWeapon();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (StoredWeapon is WeaponInstance weaponInstance)
        {
            ItemDetailPanel.Instance.Show(weaponInstance);
        }
    }
}
