using UnityEngine;
#if UNITY_EDITOR
using UnityEditor; // Sadece editörde çalýþacak kütüphane
#endif

public class QuitButtonsc : MonoBehaviour
{
    
    public void OyundanCik()
    {
        Debug.Log("Oyundan çýktý");

#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
        
        // 3. Eðer gerçek bir oyun (Build) isek uygulamayý kapat
#else
        Application.Quit();
#endif
    }
}
