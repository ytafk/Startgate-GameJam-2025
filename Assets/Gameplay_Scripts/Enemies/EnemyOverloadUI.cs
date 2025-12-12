using UnityEngine;
using UnityEngine.UI;

public class EnemyOverloadUI : MonoBehaviour
{
    public Slider slider;

    public void SetValue(float normalized)
    {
        slider.value = normalized;
    }

    void LateUpdate()
    {
        // Kameraya bakmasýn dedik ama UI sabit dursun
        // world-space slider olduðu için gerekirse eklenir
    }
}

