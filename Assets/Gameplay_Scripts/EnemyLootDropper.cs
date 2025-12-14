using UnityEngine;

public class EnemyLootDropper : MonoBehaviour
{
    public LootDropTable table;

    [Header("Scatter")]
    public float scatterRadius = 0.6f;     // etrafa yayýlma
    public float popForce = 1.5f;          // hafif fýrlatma
    public bool usePhysicsPop = true;      // prefab Rigidbody2D taþýyorsa

    public void Drop()
    {
        if (table == null || table.items == null) return;

        foreach (var it in table.items)
        {
            if (it == null || it.prefab == null) continue;

            if (Random.value > it.chance) continue;

            int count = Random.Range(it.minCount, it.maxCount + 1);
            for (int i = 0; i < count; i++)
            {
                Vector2 offset = Random.insideUnitCircle * scatterRadius;
                Vector3 pos = transform.position + (Vector3)offset;

                var go = Instantiate(it.prefab, pos, Quaternion.identity);

                if (usePhysicsPop)
                {
                    var rb = go.GetComponent<Rigidbody2D>();
                    if (rb != null)
                    {
                        Vector2 dir = offset.sqrMagnitude > 0.0001f ? offset.normalized : Random.insideUnitCircle.normalized;
                        rb.AddForce(dir * popForce, ForceMode2D.Impulse);
                    }
                }
            }
        }
    }

}
