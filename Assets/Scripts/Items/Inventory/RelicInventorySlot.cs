using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RelicInventorySlot : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] public RelicInstance StoredRelic { get; private set; }
    [SerializeField] List<Sprite> relicSprites;
    [SerializeField] private Dictionary<RelicRarity, Sprite> relicSpriteByRarity;
    [SerializeField] private Image relicImage;

    private void Awake()
    {
        relicSpriteByRarity = new Dictionary<RelicRarity, Sprite>();
        for (int i = 0; i < relicSprites.Count; i++)
        {
            RelicRarity rarity = (RelicRarity)i;
            relicSpriteByRarity[rarity] = relicSprites[i];
        }
    }
    public bool IsEmpty => StoredRelic == null;

    public void SetRelic(RelicInstance instance)
    {
        StoredRelic = instance;
        ShowRelic();
    }

    public void ShowRelic()
    {
        if(IsEmpty) return;
        if(StoredRelic.IsEmpty){
            relicImage.sprite = null;
            relicImage.color = new Color(1, 1, 1, 0);
            return;
        }
        else
        {
            relicImage.sprite = relicSpriteByRarity[StoredRelic.rarity];
            relicImage.color = new Color(1, 1, 1, 1);
        }
    }
    public void ClearSlot()
    {
        StoredRelic = null;
        ShowRelic();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        ItemDetailPanel.Instance.Show(this.StoredRelic);
    }
}
