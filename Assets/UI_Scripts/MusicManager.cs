using UnityEngine;
using System.Collections;

public class MusicManager : MonoBehaviour
{
    public static MusicManager instance;

    public AudioSource audioSource; // Inspector'dan atayýn
    public float fadeDuration = 1f; // Fade in/out süresi

    void Awake()
    {
        // Singleton kontrolü
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        // Eðer sahnede sessiz veya duruyorsa müziði baþlat
        if (audioSource != null && !audioSource.isPlaying)
        {
            audioSource.Play();
        }
    }

    // Yeni müzik çal
    public void PlayMusic(AudioClip newClip)
    {
        if (audioSource.clip == newClip) return; // Ayný müzikse deðiþtirme
        StartCoroutine(FadeMusic(newClip));
    }

    private IEnumerator FadeMusic(AudioClip newClip)
    {
        // Fade out mevcut müzik
        if (audioSource.isPlaying)
        {
            float startVolume = audioSource.volume;
            for (float t = 0; t < fadeDuration; t += Time.deltaTime)
            {
                audioSource.volume = Mathf.Lerp(startVolume, 0, t / fadeDuration);
                yield return null;
            }
            audioSource.volume = 0;
            audioSource.Stop();
        }

        // Yeni müzik set et ve çal
        audioSource.clip = newClip;
        audioSource.Play();

        // Fade in
        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            audioSource.volume = Mathf.Lerp(0, 1, t / fadeDuration);
            yield return null;
        }
        audioSource.volume = 1;
    }

    public void SetVolume(float value)
    {
        // Scrollbar 0-100 arasý çalýþýyorsa
        // audioSource.volume = value / 100f;

        // Scrollbar 0-1 arasý çalýþýyorsa (default Unity davranýþý)
        audioSource.volume = value;
    }

}
