using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingSound : MonoBehaviour
{
    [SerializeField] private Slider musicSlider;
    [SerializeField] private TextMeshProUGUI musicValue;
    [SerializeField] private Slider VFXSlider;
    [SerializeField] private TextMeshProUGUI VFXValue;

    public void UpdateSoundSetting()
    {
        //music
        musicValue.text = (musicSlider.value *100f).ToString("F0") + "%";
        SoundManager.Instance.SetMusicVolume(musicSlider.value);
        //VFX
        VFXValue.text = (VFXSlider.value *100f).ToString("F0") + "%";
        SoundManager.Instance.SetSfxVolume(VFXSlider.value);
    }
}
