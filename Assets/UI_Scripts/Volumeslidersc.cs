using UnityEngine;
using UnityEngine.UI;

public class Volumeslidersc : MonoBehaviour
{
    public Slider slider;

    private void Start()
    {
        if (slider != null && MusicManager.instance != null)
        {
            // Scrollbar baþlangýç deðerini güncel volume’ye eþitle
            slider.value = MusicManager.instance.audioSource.volume;
            // Deðiþim olduðunda müzik sesini güncelle
            slider.onValueChanged.AddListener(OnScrollValueChanged);
        }
    }

    private void OnScrollValueChanged(float value)
    {
        if (MusicManager.instance != null)
        {
            // Scrollbar 0–1 arasý çalýþýyor, sen 0–100 istiyorsan burada çarpabilirsin
            MusicManager.instance.SetVolume(value);
        }
    }
}
