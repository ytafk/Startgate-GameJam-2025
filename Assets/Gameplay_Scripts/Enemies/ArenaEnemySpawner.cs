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
    public float spawnDelayBetweenEach = 0.08f;

    [Header("Wave Settings")]
    public int startWave = 1;

    // Wave baþýna toplam kaç düþman üretilecek?
    public int startTotalThisWave = 6;
    public int addTotalPerWave = 3;

    // Ayný anda sahnede en fazla kaç düþman canlý dursun?
    public int startMaxAlive = 4;
    public int addMaxAlivePerWave = 1;

    [Header("Timing")]
    public float timeBetweenWaves = 2.0f;   // wave bitince bekleme
    public float spawnCheckInterval = 0.15f;

    // runtime
    readonly HashSet<EnemyRobot> alive = new HashSet<EnemyRobot>();
    int waveIndex;
    int totalToSpawnThisWave;
    int spawnedThisWave;
    int maxAliveThisWave;

    bool waveRunning;
    Coroutine loopCo;

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

        waveIndex = Mathf.Max(1, startWave - 1);
        StartNextWave();
    }

    void HandleEnemyDied(EnemyRobot e)
    {
        if (e != null) alive.Remove(e);
        CleanupNulls();
    }

    IEnumerator Loop()
    {
        while (true)
        {
            CleanupNulls();

            if (waveRunning)
            {
                // Wave bitti mi? (bu wave için spawn edilecekler bitti + sahnede kimse kalmadý)
                if (spawnedThisWave >= totalToSpawnThisWave && alive.Count == 0)
                {
                    waveRunning = false;
                    yield return new WaitForSeconds(timeBetweenWaves);
                    StartNextWave();
                }
                else
                {
                    // Spawn etmemiz gerekiyorsa: (hala spawn hakkýmýz var) + (alive limiti dolmadý)
                    while (spawnedThisWave < totalToSpawnThisWave && alive.Count < maxAliveThisWave)
                    {
                        SpawnOne();
                        spawnedThisWave++;
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

        totalToSpawnThisWave = Mathf.Max(0, startTotalThisWave + (waveIndex - 1) * addTotalPerWave);
        maxAliveThisWave = Mathf.Max(1, startMaxAlive + (waveIndex - 1) * addMaxAlivePerWave);

        spawnedThisWave = 0;
        waveRunning = true;

        // Ýstersen burada UI'ya waveIndex'i basarsýn
        // Debug.Log($"WAVE {waveIndex} baþladý | total:{totalToSpawnThisWave} | maxAlive:{maxAliveThisWave}");
    }

    void CleanupNulls()
    {
        alive.RemoveWhere(x => x == null);
    }

    void SpawnOne()
    {
        if (!enemyPrefab || spawnPoints == null || spawnPoints.Length == 0) return;

        Transform sp = PickSpawnPoint();
        Vector3 pos = sp.position;

        var go = Instantiate(enemyPrefab, pos, Quaternion.identity);
        var er = go.GetComponent<EnemyRobot>();
        if (er != null) alive.Add(er);
    }

    Transform PickSpawnPoint()
    {
        if (!player) return spawnPoints[Random.Range(0, spawnPoints.Length)];

        List<Transform> candidates = new List<Transform>();
        foreach (var sp in spawnPoints)
        {
            if (!sp) continue;
            if (Vector2.Distance(sp.position, player.position) >= minDistanceFromPlayer)
                candidates.Add(sp);
        }

        if (candidates.Count > 0)
            return candidates[Random.Range(0, candidates.Count)];

        return spawnPoints[Random.Range(0, spawnPoints.Length)];
    }
}
