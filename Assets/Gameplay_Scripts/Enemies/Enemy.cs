using UnityEngine;

public class EnemyRobot : MonoBehaviour
{
    [Header("Overload Settings")]
    public float maxOverload = 42f;
    public float currentOverload = 0f;

    [Header("Optional")]
    public float overloadDecayPerSecond = 0f; // istersek zamanla azalsýn (þimdilik 0)

    [Header("FX")]
    public GameObject explosionPrefab;

    public void AddOverload(float amount)
    {
        currentOverload += amount;
        currentOverload = Mathf.Clamp(currentOverload, 0f, maxOverload);

        UpdateUI();

        if (currentOverload >= maxOverload)
        {
            Explode();
        }
    }

    void Update()
    {
        // Ýleride istersen aþýrý yük zamanla düþsün
        if (overloadDecayPerSecond > 0f && currentOverload > 0f)
        {
            currentOverload -= overloadDecayPerSecond * Time.deltaTime;
            currentOverload = Mathf.Max(0f, currentOverload);
            UpdateUI();
        }
    }

    void Explode()
    {
        if (explosionPrefab)
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);

        Destroy(gameObject);
    }

    // UI baðlantýsý
    EnemyOverloadUI ui;
    void Awake()
    {
        ui = GetComponentInChildren<EnemyOverloadUI>();
        UpdateUI();
    }

    void UpdateUI()
    {
        if (ui)
            ui.SetValue(currentOverload / maxOverload);
    }
}
