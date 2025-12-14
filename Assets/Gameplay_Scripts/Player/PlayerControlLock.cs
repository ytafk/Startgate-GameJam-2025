using UnityEngine;

public class PlayerControlLock : MonoBehaviour
{
    Behaviour[] toDisable;

    public void Lock()
    {
        // Buraya senin player kontrol scriptlerin girer:
        // Movement, inventory/fire, dash vs.
        // Hepsini otomatik bulup kapatacaðýz.

        toDisable = new Behaviour[]
        {
            GetComponent<PlayerMovement2D_NewInput>(),
            GetComponent<PlayerWeaponInventory>(),
            
        };

        foreach (var b in toDisable)
            if (b) b.enabled = false;

        // Fizik hareketini durdur
        var rb = GetComponent<Rigidbody2D>();
        if (rb)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
    }
}


