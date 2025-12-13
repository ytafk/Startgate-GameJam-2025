using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic; // Listeleri kullanmak için gerekli

public class CursorYoneticisi : MonoBehaviour
{
    public static CursorYoneticisi instance; // Singleton yapýsý için

    [System.Serializable]
    public struct OzelSahneAyari
    {
        public string sahneIsmi;      // Hangi sahnede? (Örn: "OyunSahnesi")
        public Texture2D cursorGorseli; // Hangi görsel?
        public Vector2 tiklamaNoktasi;  // Uç noktasý neresi? (Örn: Niþangah için orta nokta)
    }

    [Header("Genel Ayarlar (Varsayýlan)")]
    public Texture2D varsayilanCursor; // Listede adý olmayan sahnelerde bu görünür
    public Vector2 varsayilanTiklamaNoktasi;

    [Header("Özel Sahne Listesi")]
    [Tooltip("Hangi sahnede hangi görselin çýkacaðýný buradan ekle")]
    public List<OzelSahneAyari> ozelSahneler;

    void Awake()
    {
        // Singleton: Bu objeden sahnede sadece 1 tane olmasýný garantiler
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // Sahne deðiþse bile bu obje yok olmaz!
        }
        else
        {
            Destroy(gameObject); // Eðer 2. bir yönetici oluþursa onu yok et
        }
    }

    void OnEnable()
    {
        // Sahne yüklendiðinde tetiklenecek olayý dinlemeye baþla
        SceneManager.sceneLoaded += SahneYuklendi;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= SahneYuklendi;
    }

    // Her sahne deðiþtiðinde Unity bu fonksiyonu otomatik çalýþtýrýr
    void SahneYuklendi(Scene scene, LoadSceneMode mode)
    {
        CursorDegistir(scene.name);
    }

    void CursorDegistir(string suankiSahneAdi)
    {
        // 1. Önce listeyi kontrol et: Þu anki sahne ismi listede var mý?
        foreach (var ayar in ozelSahneler)
        {
            if (ayar.sahneIsmi == suankiSahneAdi)
            {
                // Bulduk! Bu sahne için özel görseli ayarla
                Cursor.SetCursor(ayar.cursorGorseli, ayar.tiklamaNoktasi, CursorMode.Auto);
                return; // Ýþlem tamam, fonksiyondan çýk
            }
        }

        // 2. Eðer listede bu sahne yoksa, varsayýlan (Default) görseli yükle
        Cursor.SetCursor(varsayilanCursor, varsayilanTiklamaNoktasi, CursorMode.Auto);
    }
}
