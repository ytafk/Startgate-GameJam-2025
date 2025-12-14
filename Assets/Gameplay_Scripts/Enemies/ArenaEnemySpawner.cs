//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public class ArenaWaveSpawner : MonoBehaviour
//{
//    [Header("Refs")]
//    public Transform player;

//    [Header("Spawn Ayarları")]
//    public GameObject enemyPrefab;
//    public Transform[] spawnPoints;
//    public float minDistanceFromPlayer = 6f;

//    [Tooltip("Düşman öldükten sonra yenisinin gelmesi için kaç saniye beklensin? (Önemli)")]
//    public float spawnDelay = 2.0f; // Burayı 1.5 veya 2.0 yaparsan 'anlık düşme' hissini net görürsün.

//    [Header("Wave Zorluk Ayarları")]
//    public int baseEnemyCount = 9;
//    public int increasePerWave = 3;

//    public int baseMaxAlive = 5;
//    public int increaseMaxAlivePerWave = 1;

//    [Header("Timing")]
//    public float timeBetweenWaves = 3.0f;
//    public float spawnCheckInterval = 0.5f;

//    // Runtime Değişkenler
//    readonly HashSet<EnemyRobot> alive = new HashSet<EnemyRobot>();
//    int waveIndex = 0;
//    int totalToSpawnThisWave;
//    int spawnedThisWave;
//    int maxAliveThisWave;

//    bool waveRunning;
//    Coroutine loopCo;

//    // ✅ SENİN İSTEDİĞİN: Sadece canlı olanları gösterir (4 -> 3 -> 4 efekti için)
//    // Eğer "Toplam kalan" istersen burayı değiştirebiliriz.
//    public int EnemiesVisible => alive.Count;

//    void OnEnable()
//    {
//        EnemyRobot.OnAnyEnemyDied += HandleEnemyDied;
//        loopCo = StartCoroutine(Loop());
//    }

//    void OnDisable()
//    {
//        EnemyRobot.OnAnyEnemyDied -= HandleEnemyDied;
//        if (loopCo != null) StopCoroutine(loopCo);
//    }

//    void Start()
//    {
//        if (!player)
//        {
//            var p = GameObject.FindGameObjectWithTag("Player");
//            if (p) player = p.transform;
//        }
//        StartNextWave();
//    }

//    void HandleEnemyDied(EnemyRobot e)
//    {
//        if (e != null) alive.Remove(e);
//        CleanupNulls();

//        // Öldüğü an UI güncellensin (Sayı düşsün)
//        UpdateUI();
//    }

//    IEnumerator Loop()
//    {
//        while (true)
//        {
//            CleanupNulls();

//            if (waveRunning)
//            {
//                // Wave BİTTİ Mİ? (Hem spawn hakkı bitti hem de sahnede kimse kalmadı)
//                if (spawnedThisWave >= totalToSpawnThisWave && alive.Count == 0)
//                {
//                    waveRunning = false;
//                    Debug.Log("Wave Bitti!");
//                    yield return new WaitForSeconds(timeBetweenWaves);
//                    StartNextWave();
//                }
//                else
//                {
//                    // SPAWN GEREKİYOR MU?
//                    // Üretilecek adam var mı? VE Sahnede yer var mı?
//                    if (spawnedThisWave < totalToSpawnThisWave && alive.Count < maxAliveThisWave)
//                    {
//                        // ✅ İŞTE ÇÖZÜM BURADA:
//                        // Birisi öldüğünde hemen spamlamasın, biraz beklesin ki sayının düştüğünü görelim.
//                        yield return new WaitForSeconds(spawnDelay);

//                        // Bekledikten sonra hala yer varsa spawn et
//                        if (alive.Count < maxAliveThisWave)
//                        {
//                            SpawnOne();
//                            spawnedThisWave++;
//                            UpdateUI(); // Doğunca sayı geri artsın
//                        }
//                    }
//                }
//            }

//            yield return new WaitForSeconds(spawnCheckInterval);
//        }
//    }

//    void StartNextWave()
//    {
//        waveIndex++;
//        totalToSpawnThisWave = baseEnemyCount + (waveIndex - 1) * increasePerWave;
//        maxAliveThisWave = baseMaxAlive + (waveIndex - 1) * increaseMaxAlivePerWave;
//        spawnedThisWave = 0;
//        waveRunning = true;

//        if (WeaponUIManager.Instance != null)
//        {
//            WeaponUIManager.Instance.UpdateWaveUI(waveIndex);
//        }
//        UpdateUI();
//    }

//    void UpdateUI()
//    {
//        if (WeaponUIManager.Instance != null)
//        {
//            // Canlı sayısını (alive.Count) gönderiyoruz.
//            // Böylece öldürünce azalır, doğunca artar.
//            WeaponUIManager.Instance.UpdateEnemyCountUI(alive.Count);
//        }
//    }

//    void CleanupNulls()
//    {
//        alive.RemoveWhere(x => x == null);
//    }

//    void SpawnOne()
//    {
//        if (!enemyPrefab || spawnPoints == null || spawnPoints.Length == 0) return;

//        Transform sp = PickSpawnPoint();
//        var go = Instantiate(enemyPrefab, sp.position, Quaternion.identity);

//        var er = go.GetComponent<EnemyRobot>();
//        if (er == null) er = go.GetComponentInChildren<EnemyRobot>();

//        if (er != null) alive.Add(er);
//    }

//    Transform PickSpawnPoint()
//    {
//        if (!player) return spawnPoints[Random.Range(0, spawnPoints.Length)];

//        List<Transform> candidates = new List<Transform>();
//        foreach (var sp in spawnPoints)
//        {
//            if (!sp) continue;
//            if (Vector2.Distance(sp.position, player.position) >= minDistanceFromPlayer)
//                candidates.Add(sp);
//        }

//        if (candidates.Count > 0)
//            return candidates[Random.Range(0, candidates.Count)];

//        return spawnPoints[Random.Range(0, spawnPoints.Length)];
//    }
//}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArenaWaveSpawner : MonoBehaviour
{
    [Header("Refs")]
    public Transform player;

    [Header("Spawn")]
    public GameObject enemyPrefab;
    public Transform[] spawnPoints;
    public float minDistanceFromPlayer = 6f;
    public float spawnDelayBetweenEach = 0.5f; // Düşmanlar üst üste binmesin diye biraz artırdım

    [Header("Wave Zorluk Ayarları")]
    [Tooltip("İlk dalgada kaç düşman olacak? (Inspector'dan ayarla)")]
    public int baseEnemyCount = 9; // Sen 9 istiyorsun, burayı 9 yaptım.

    [Tooltip("Her dalgada düşman sayısı kaç artacak?")]
    public int increasePerWave = 3;

    [Tooltip("Aynı anda sahnede en fazla kaç düşman olabilir?")]
    public int baseMaxAlive = 5;
    public int increaseMaxAlivePerWave = 1;

    [Header("Timing")]
    public float timeBetweenWaves = 3.0f;
    public float spawnCheckInterval = 0.2f;

    // Runtime Değişkenler
    readonly HashSet<EnemyRobot> alive = new HashSet<EnemyRobot>();
    int waveIndex = 0;
    int totalToSpawnThisWave;
    int spawnedThisWave;
    int maxAliveThisWave;

    bool waveRunning;
    Coroutine loopCo;

    // Kalan Düşman Hesabı: (Toplam Üretilecek - Üretilmiş Olan) + Şu An Sahnede Canlı Olan
    public int EnemiesRemaining => (totalToSpawnThisWave - spawnedThisWave) + alive.Count;

    void OnEnable()
    {
        EnemyRobot.OnAnyEnemyDied += HandleEnemyDied;
        loopCo = StartCoroutine(Loop());
    }

    void OnDisable()
    {
        EnemyRobot.OnAnyEnemyDied -= HandleEnemyDied;
        if (loopCo != null) StopCoroutine(loopCo);
    }

    void Start()
    {
        if (!player)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p) player = p.transform;
        }

        // Oyunu başlat
        StartNextWave();
    }

    // Garanti olsun diye her karede olmasa da sık sık UI'ı zorla güncelle
    // (Bazen event sistemi kaçırırsa burası yakalar)
    void Update()
    {
        if (waveRunning && WeaponUIManager.Instance != null)
        {
            // Bu satır performans yemez ama UI'ın takılmamasını %100 garanti eder
            WeaponUIManager.Instance.UpdateEnemyCountUI(EnemiesRemaining);
        }
    }

    void HandleEnemyDied(EnemyRobot e)
    {
        if (e != null) alive.Remove(e);
        CleanupNulls();

        // UI Güncelle (Update'de de yapıyoruz ama anlık tepki için burada da dursun)
        UpdateUI();
    }

    IEnumerator Loop()
    {
        while (true)
        {
            CleanupNulls();

            if (waveRunning)
            {
                // Wave BİTİŞ KONTROLÜ
                // Hem üretilecekler bitti (spawned >= total) HEM DE sahnede kimse kalmadı (alive == 0)
                if (spawnedThisWave >= totalToSpawnThisWave && alive.Count == 0)
                {
                    waveRunning = false;
                    Debug.Log($"Wave {waveIndex} Bitti! Sıradaki hazırlanıyor...");
                    yield return new WaitForSeconds(timeBetweenWaves);
                    StartNextWave();
                }
                else
                {
                    // SPAWN KONTROLÜ
                    // Daha üretilecek adam var mı? VE Sahnede yer var mı?
                    while (spawnedThisWave < totalToSpawnThisWave && alive.Count < maxAliveThisWave)
                    {
                        SpawnOne();
                        spawnedThisWave++;

                        // Her spawn arasında bekle
                        if (spawnDelayBetweenEach > 0f)
                            yield return new WaitForSeconds(spawnDelayBetweenEach);
                    }
                }
            }

            yield return new WaitForSeconds(spawnCheckInterval);
        }
    }

    void StartNextWave()
    {
        waveIndex++;

        // --- MATEMATİKSEL HESAPLAMA ---
        // Wave 1 için: 9 + (0 * 3) = 9 düşman
        // Wave 2 için: 9 + (1 * 3) = 12 düşman
        totalToSpawnThisWave = baseEnemyCount + (waveIndex - 1) * increasePerWave;

        // Max Alive (Aynı anda sahnede kaç kişi): 5 + (wave-1)
        maxAliveThisWave = baseMaxAlive + (waveIndex - 1) * increaseMaxAlivePerWave;

        spawnedThisWave = 0;
        waveRunning = true;

        Debug.Log($"WAVE {waveIndex} BAŞLADI. Toplam Düşman: {totalToSpawnThisWave}");

        // UI Başlangıç Güncellemesi
        if (WeaponUIManager.Instance != null)
        {
            WeaponUIManager.Instance.UpdateWaveUI(waveIndex);
            UpdateUI();
        }
    }

    void UpdateUI()
    {
        if (WeaponUIManager.Instance != null)
        {
            WeaponUIManager.Instance.UpdateEnemyCountUI(EnemiesRemaining);
        }
    }

    void CleanupNulls()
    {
        alive.RemoveWhere(x => x == null);
    }

    void SpawnOne()
    {
        if (!enemyPrefab || spawnPoints == null || spawnPoints.Length == 0) return;

        Transform sp = PickSpawnPoint();
        var go = Instantiate(enemyPrefab, sp.position, Quaternion.identity);

        // Scripti bul (Hem ana objede hem child'larda arar)
        var er = go.GetComponent<EnemyRobot>();
        if (er == null) er = go.GetComponentInChildren<EnemyRobot>();

        if (er != null)
        {
            alive.Add(er);
        }
        else
        {
            Debug.LogError("HATA: Spawn edilen prefabda EnemyRobot scripti yok!");
        }
    }

    Transform PickSpawnPoint()
    {
        if (!player) return spawnPoints[Random.Range(0, spawnPoints.Length)];

        List<Transform> candidates = new List<Transform>();
        foreach (var sp in spawnPoints)
        {
            if (!sp) continue;
            // Oyuncuya çok yakın olanları ele
            if (Vector2.Distance(sp.position, player.position) >= minDistanceFromPlayer)
                candidates.Add(sp);
        }

        if (candidates.Count > 0)
            return candidates[Random.Range(0, candidates.Count)];

        return spawnPoints[Random.Range(0, spawnPoints.Length)];
    }
}