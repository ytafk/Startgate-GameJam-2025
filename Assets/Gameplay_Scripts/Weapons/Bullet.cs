using UnityEngine;

public class Bullet : MonoBehaviour
{
<<<<<<< Updated upstream
    public float overloadAmount = 4f;
    public float lifeTime = 2f;

    void Start()
    {
        Destroy(gameObject, lifeTime);
=======
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
>>>>>>> Stashed changes
    }

    void OnTriggerEnter2D(Collider2D other)
    {
<<<<<<< Updated upstream
        EnemyRobot enemy = other.GetComponentInParent<EnemyRobot>();
        if (enemy)
=======
        var enemy = other.GetComponentInParent<EnemyRobot>();
        if (enemy != null)
>>>>>>> Stashed changes
        {
            enemy.AddOverload(overloadAmount);
            Destroy(gameObject);
        }
    }
}
