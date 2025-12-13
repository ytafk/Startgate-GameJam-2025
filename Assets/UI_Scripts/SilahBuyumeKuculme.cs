using UnityEngine;

public class SilahBuyumeKuculme : MonoBehaviour
{
    [Header("Ayarlar")]
    [Tooltip("Ne kadar hýzlý büyüyüp küçülecek?")]
    public float hiz = 5f;

    [Tooltip("Ne kadar büyüyecek? (0.1 = %10 büyüme)")]
    public float guc = 0.05f;

    private Vector3 baslangicBoyutu;

    void Start()
    {
        // Oyun baþladýðýnda objenin orijinal boyutunu hafýzaya al
        baslangicBoyutu = transform.localScale;
    }

    void Update()
    {
        // Sinüs dalgasý -1 ile 1 arasýnda gider gelir.
        // Bunu zamanla çarparak sürekli bir döngü oluþturuyoruz.
        float sinyal = Mathf.Sin(Time.time * hiz);

        // Orijinal boyuta, hesapladýðýmýz küçük deðiþim miktarýný ekliyoruz
        transform.localScale = baslangicBoyutu + (Vector3.one * sinyal * guc);
    }
}
