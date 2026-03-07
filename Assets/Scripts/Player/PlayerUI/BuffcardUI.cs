using UnityEngine;
using UnityEngine.UI;

public class BuffCardUI : MonoBehaviour
{
    public Image icon;
    public TMPro.TextMeshProUGUI durationText;

    public void SetBuff(BaseBuff buff)
    {
        icon.sprite = buff.data.buffIcon;
        string buffTextDisplay = $"{buff.duration:F1} {buff.data.name}";

        durationText.text = buffTextDisplay;
    }
}