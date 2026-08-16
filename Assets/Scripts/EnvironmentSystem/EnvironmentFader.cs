using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class EnvironmentFader : MonoBehaviour
{
    private Transform player;

    private SpriteRenderer spriteRenderer;
    private Color spriteColor;

    [Header("Distance Settings")]
    public float startFadeDistance = 10f;
    public float fullVisibleDistance = 3f;

    [Header("Behavior Settings")]
    public bool stayVisibleOnceRevealed = false;

    private float maxAlphaReached = 0f;
    private float startFadeSqr;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteColor = spriteRenderer.color;

        startFadeSqr = startFadeDistance * startFadeDistance;

        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) player = playerObj.transform;
        }
    }

    void Update()
    {
        if (player == null) return;

        Vector2 offset = transform.position - player.position;

        if (offset.sqrMagnitude > startFadeSqr && maxAlphaReached == 0f)
        {
            if (spriteColor.a > 0) SetAlpha(0f);
            return;
        }

        float distance = offset.magnitude;
        float targetAlpha = 1f - Mathf.InverseLerp(fullVisibleDistance, startFadeDistance, distance);

        if (stayVisibleOnceRevealed)
        {
            if (targetAlpha > maxAlphaReached)
            {
                maxAlphaReached = targetAlpha;
            }
            targetAlpha = maxAlphaReached;

            if (targetAlpha >= 0.99f)
            {
                SetAlpha(1f);
                this.enabled = false;
                return;
            }
        }

        if (Mathf.Abs(spriteColor.a - targetAlpha) > 0.01f)
        {
            SetAlpha(targetAlpha);
        }
    }

    private void SetAlpha(float alpha)
    {
        spriteColor.a = alpha;
        spriteRenderer.color = spriteColor;
    }
}