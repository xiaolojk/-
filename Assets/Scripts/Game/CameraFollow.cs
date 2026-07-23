using UnityEngine;

/// <summary>
/// 摄像机跟随
/// </summary>
public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public float smoothSpeed = 0.125f;
    public Vector3 offset = new Vector3(0, 0, -10);

    void Start()
    {
        if (target == null)
        {
            var player = FindObjectOfType<PlayerController>();
            if (player != null) target = player.transform;
        }
    }

    void LateUpdate()
    {
        if (target == null) return;
        Vector3 desired = target.position + offset;
        transform.position = Vector3.Lerp(transform.position, desired, smoothSpeed);
    }
}