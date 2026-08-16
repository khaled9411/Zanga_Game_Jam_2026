using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraEffects : MonoBehaviour
{
    [Header("Attack Zoom")]
    [SerializeField] private float zoomPunchAmount = 0.5f;
    [SerializeField] private float zoomPunchDuration = 0.15f;

    [Header("Rewind Shake")]
    [SerializeField] private float shakeAmount = 0.15f;
    [SerializeField] private float shakeDuration = 0.1f;

    [Header("Death Zoom")]
    [SerializeField] private float deathZoomTargetSize = 2.5f;
    [SerializeField] private float deathZoomDuration = 1.2f;
    [SerializeField] private CameraFollow cameraFollow; // drag the same Main Camera's CameraFollow here

    private Camera cam;
    private float baseOrthoSize;
    private Coroutine zoomRoutine;
    private Coroutine shakeRoutine;
    private Coroutine deathZoomRoutine;

    private void Awake()
    {
        cam = GetComponent<Camera>();
        baseOrthoSize = cam.orthographicSize;
    }

    private void OnEnable()
    {
        GameEvents.OnWeaponAttackUsed += HandleAttackUsed;
        GameEvents.OnRewindKeyPressed += HandleRewindKeyPressed;
        GameEvents.OnHeartsChanged += HandleHeartsChanged;
    }

    private void OnDisable()
    {
        GameEvents.OnWeaponAttackUsed -= HandleAttackUsed;
        GameEvents.OnRewindKeyPressed -= HandleRewindKeyPressed;
        GameEvents.OnHeartsChanged -= HandleHeartsChanged;
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

    private void HandleHeartsChanged(int hearts)
    {
        if (hearts <= 0)
        {
            if (deathZoomRoutine != null) StopCoroutine(deathZoomRoutine);
            deathZoomRoutine = StartCoroutine(DeathZoom());
        }
    }


    private IEnumerator DeathZoom()
    {
        if (cameraFollow != null)
            cameraFollow.enabled = false; // stop the lagged follow so our direct move isn't fought

        Transform target = cameraFollow != null ? cameraFollow.GetTarget() : null;

        float t = 0f;
        float startSize = cam.orthographicSize;
        Vector3 startPos = transform.position;
        Vector3 targetPos = target != null
            ? new Vector3(target.position.x, target.position.y, transform.position.z)
            : transform.position;

        while (t < deathZoomDuration)
        {
            t += Time.unscaledDeltaTime;
            float progress = t / deathZoomDuration;

            cam.orthographicSize = Mathf.Lerp(startSize, deathZoomTargetSize, progress);
            transform.position = Vector3.Lerp(startPos, targetPos, progress);

            yield return null;
        }

        cam.orthographicSize = deathZoomTargetSize;
        transform.position = targetPos;
    }
}