using UnityEngine;

/// <summary>
/// Smoothly follows a target with a slight lag, for a dreamy floaty feel.
/// Attach to Main Camera. Assign the Player transform in the Inspector.
/// </summary>
public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float smoothTime = 0.35f; // higher = more delay/lag
    [SerializeField] private Vector3 offset = new Vector3(0f, 0f, -10f);

    private Vector3 velocity = Vector3.zero;

    // Used by CameraEffects for punch-zoom without fighting this script's follow logic
    private float zoomOffset = 0f;

    private void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPos = target.position + offset;
        transform.position = Vector3.SmoothDamp(transform.position, desiredPos, ref velocity, smoothTime);
    }

    public void SetTarget(Transform newTarget) => target = newTarget;
}