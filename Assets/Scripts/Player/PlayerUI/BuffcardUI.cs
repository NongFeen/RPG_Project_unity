using UnityEngine;
using UnityEngine.UI;

public class BuffCardUI : MonoBehaviour
{
    public Image icon;
    public TMPro.TextMeshProUGUI durationText;

    public void SetBuff(BaseBuff buff)
    {
        // icon.sprite = buff.icon;
        durationText.text = buff.duration.ToString("F1");
    }
}