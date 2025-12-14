using System.Collections;
using UnityEngine;

public class EnemyHitFlash : MonoBehaviour
{
    [Header("Flash")]
    public Color hitColor = Color.red;
    public float flashDuration = 0.08f;

    SpriteRenderer[] renderers;
    Color[] originalColors;
    Coroutine co;

    void Awake()
    {
        // Enemy altýnda birden fazla sprite olabilir diye
        renderers = GetComponentsInChildren<SpriteRenderer>();

        originalColors = new Color[renderers.Length];
        for (int i = 0; i < renderers.Length; i++)
            originalColors[i] = renderers[i].color;
    }

    public void Flash()
    {
        if (co != null)
            StopCoroutine(co);

        co = StartCoroutine(FlashRoutine());
    }

    IEnumerator FlashRoutine()
    {
        // kýrmýzýya al
        for (int i = 0; i < renderers.Length; i++)
            renderers[i].color = hitColor;

        yield return new WaitForSeconds(flashDuration);

        // eski rengine dön
        for (int i = 0; i < renderers.Length; i++)
            renderers[i].color = originalColors[i];

        co = null;
    }
}
