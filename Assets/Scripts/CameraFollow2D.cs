using UnityEngine;

public class CameraFollow2D : MonoBehaviour
{
    [Header("Target")]
    public Transform target;

    [Header("Follow Settings")]
    public float smoothTime = 0.2f;
    public Vector2 offset = new Vector2(2f, 0f);

    [Header("Axis Control")]
    public bool followX = true;
    public bool followY = false;

    private Vector3 velocity = Vector3.zero;

    private void LateUpdate()
    {
        if (target == null) return;

        float targetX = followX ? target.position.x + offset.x : transform.position.x;
        float targetY = followY ? target.position.y + offset.y : transform.position.y;

        Vector3 targetPosition = new Vector3(targetX, targetY, transform.position.z);

        transform.position = Vector3.SmoothDamp(
            transform.position,
            targetPosition,
            ref velocity,
            smoothTime
        );
    }
}