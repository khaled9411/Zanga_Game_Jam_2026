using UnityEngine;

public class RewindPickup : MonoBehaviour
{
    [SerializeField] private float interactRange = 1.5f;
    [SerializeField] private float fillPerPress = 15f;
    [SerializeField] private float decayPerSecond = 20f;

    private Transform player;
    private bool mashingActive = false;
    private float fillAmount = 0f;

    private void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;
    }

    private void Update()
    {
        if (player == null) return;

        float distance = Vector2.Distance(transform.position, player.position);
        bool inRange = distance <= interactRange;
        bool anyKeyPressed = Input.anyKeyDown; // any keyboard/mouse key, not just E

        if (!mashingActive)
        {
            if (inRange && anyKeyPressed)
            {
                mashingActive = true;
                Debug.Log("[RewindPickup] Mash started — spam keys to fill the bar!");
            }
            return;
        }

        if (!inRange)
        {
            Debug.Log("[RewindPickup] Player left range — mash cancelled.");
            mashingActive = false;
            fillAmount = 0f;
            return;
        }

        if (anyKeyPressed)
        {
            fillAmount += fillPerPress;
            GameEvents.TriggerRewindKeyPressed();
            Debug.Log($"[RewindPickup] Spam keys! Fill: {Mathf.Clamp(fillAmount, 0, 100):0}%");
        }

        fillAmount -= decayPerSecond * Time.deltaTime;
        fillAmount = Mathf.Clamp(fillAmount, 0f, 100f);

        if (fillAmount >= 100f)
        {
            Debug.Log("[RewindPickup] Bar filled! Heart restored.");
            GameEvents.TriggerRequestRestoreHeart();
            Destroy(gameObject);
        }
    }

    public float FillPercent => fillAmount;
    public bool IsMashing => mashingActive;
}