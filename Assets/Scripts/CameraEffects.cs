using System.Collections;
using UnityEngine;

/// <summary>
/// Handles camera punch-zoom on attack and shake on rewind key-mash.
/// Attach to Main Camera alongside CameraFollow.
/// </summary>
[RequireComponent(typeof(Camera))]
public class CameraEffects : MonoBehaviour
{
    [Header("Attack Zoom")]
    [SerializeField] private float zoomPunchAmount = 0.5f;
    [SerializeField] private float zoomPunchDuration = 0.15f;

    [Header("Rewind Shake")]
    [SerializeField] private float shakeAmount = 0.15f;
    [SerializeField] private float shakeDuration = 0.1f;

    private Camera cam;
    private float baseOrthoSize;
    private Coroutine zoomRoutine;
    private Coroutine shakeRoutine;
    private Vector3 shakeOriginPos;

    private void Awake()
    {
        cam = GetComponent<Camera>();
        baseOrthoSize = cam.orthographicSize;
    }

    private void OnEnable()
    {
        GameEvents.OnWeaponAttackUsed += HandleAttackUsed;
        GameEvents.OnRewindKeyPressed += HandleRewindKeyPressed;
    }

    private void OnDisable()
    {
        GameEvents.OnWeaponAttackUsed -= HandleAttackUsed;
        GameEvents.OnRewindKeyPressed -= HandleRewindKeyPressed;
    }

    private void HandleAttackUsed()
    {
        if (zoomRoutine != null) StopCoroutine(zoomRoutine);
        zoomRoutine = StartCoroutine(ZoomPunch());
    }

    private IEnumerator ZoomPunch()
    {
        float t = 0f;
        float startSize = cam.orthographicSize;
        float targetSize = baseOrthoSize - zoomPunchAmount;

        while (t < zoomPunchDuration)
        {
            t += Time.unscaledDeltaTime;
            cam.orthographicSize = Mathf.Lerp(startSize, targetSize, t / zoomPunchDuration);
            yield return null;
        }

        t = 0f;
        while (t < zoomPunchDuration)
        {
            t += Time.unscaledDeltaTime;
            cam.orthographicSize = Mathf.Lerp(targetSize, baseOrthoSize, t / zoomPunchDuration);
            yield return null;
        }

        cam.orthographicSize = baseOrthoSize;
    }

    private void HandleRewindKeyPressed()
    {
        if (shakeRoutine != null) StopCoroutine(shakeRoutine);
        shakeRoutine = StartCoroutine(Shake());
    }

    private IEnumerator Shake()
    {
        Vector3 originalLocalPos = transform.localPosition;
        float t = 0f;

        while (t < shakeDuration)
        {
            t += Time.unscaledDeltaTime;
            float x = Random.Range(-1f, 1f) * shakeAmount;
            float y = Random.Range(-1f, 1f) * shakeAmount;
            transform.localPosition = originalLocalPos + new Vector3(x, y, 0f);
            yield return null;
        }

        transform.localPosition = originalLocalPos;
    }
}