using UnityEngine;

public class CameraFollow2D : MonoBehaviour
{
    public Transform target;
    public string playerTag = "Player";
    public float smooth = 12f;

    Vector3 offset;

    void Start()
    {
        if (!target)
        {
            var p = GameObject.FindGameObjectWithTag(playerTag);
            if (p) target = p.transform;
        }

        offset = transform.position - (target ? target.position : Vector3.zero);
    }

    void LateUpdate()
    {
        if (!target)
        {
            var p = GameObject.FindGameObjectWithTag(playerTag);
            if (p) target = p.transform;
            if (!target) return;
        }

        Vector3 desired = target.position + offset;
        desired.z = transform.position.z; // 2D için z sabit

        transform.position = Vector3.Lerp(transform.position, desired, Time.deltaTime * smooth);
    }
}
