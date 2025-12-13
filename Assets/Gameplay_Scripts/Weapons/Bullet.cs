using UnityEngine;

public class Bullet : MonoBehaviour
{
    [HideInInspector] public float overloadAmount;
    [HideInInspector] public float maxRange;

    private Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        if (Vector3.Distance(startPos, transform.position) >= maxRange)
            Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        var enemy = other.GetComponentInParent<EnemyRobot>();
        if (enemy != null)
        {
            enemy.AddOverload(overloadAmount);
            Destroy(gameObject);
        }
    }
}
