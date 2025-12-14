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
        // Menzil dýþýna çýkarsa yok et
        if (Vector3.Distance(startPos, transform.position) >= maxRange)
            Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // 1. DÜÞMAN KONTROLÜ (Senin eski kodun)
        var enemy = other.GetComponentInParent<EnemyRobot>();
        if (enemy != null)
        {
            enemy.AddOverload(overloadAmount);
            Destroy(gameObject);
            return; // Çarptýk ve yok olduk, fonksiyondan çýkalým.
        }

        // 2. DUVAR KONTROLÜ (YENÝ EKLENEN KISIM)
        // Eðer çarptýðýmýz objenin etiketi "Wall" ise mermiyi yok et
        if (other.CompareTag("Wall"))
        {
            Destroy(gameObject);
        }
    }
}

