using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public float maxHP = 100f;
    public float hp = 100f;

    public void TakeDamage(float dmg)
    {
        hp -= dmg;
        if (hp <= 0f)
        {
            hp = 0f;
            Debug.Log("Player died");
            // Ýstersen burada respawn / game over
        }
    }
}
