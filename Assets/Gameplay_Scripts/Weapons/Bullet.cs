using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float overloadAmount = 4f;
    public float lifeTime = 2f;

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        EnemyRobot enemy = other.GetComponentInParent<EnemyRobot>();
        if (enemy)
        {
            enemy.AddOverload(overloadAmount);
            Destroy(gameObject);
        }
    }
}
