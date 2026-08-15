using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class UIManager : MonoBehaviour
{
    [Header("HUD")]
    [SerializeField] private TextMeshProUGUI heartsText;
    [SerializeField] private TextMeshProUGUI clockText;

    [Header("End Screens")]
    [SerializeField] private GameObject winPanel;
    [SerializeField] private GameObject losePanel;
    [Header("Pause")]
    [SerializeField] private GameObject pausePanel;

    private int lastDisplayedInterval = -1; // tracks which 30-second block we last showed

    private void OnEnable()
    {
        GameEvents.OnHeartsChanged += HandleHeartsChanged;
        GameEvents.OnClockUpdated += HandleClockUpdated;

        GameEvents.OnGameWon += HandleGameWon;
        GameEvents.OnGameLost += HandleGameLost;
        GameEvents.OnGamePaused += HandleGamePaused;
        GameEvents.OnGameResumed += HandleGameResumed;
    }
    private void OnDisable()
    {
        GameEvents.OnHeartsChanged -= HandleHeartsChanged;
        GameEvents.OnClockUpdated -= HandleClockUpdated;

        GameEvents.OnGameWon -= HandleGameWon;
        GameEvents.OnGameLost -= HandleGameLost;
        GameEvents.OnGamePaused -= HandleGamePaused;
        GameEvents.OnGameResumed -= HandleGameResumed;
    }
    private void Start()
    {
        if (winPanel != null) winPanel.SetActive(false);
        if (losePanel != null) losePanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(false);

        lastDisplayedInterval = -1;
        if (clockText != null) clockText.text = "00:00 AM";
    }
    private void HandleHeartsChanged(int hearts)
    {
        if (heartsText != null)
            heartsText.text = $"Hearts: {hearts}";
    }
    private void HandleClockUpdated(float remainingSeconds)
    {
        if (clockText == null) return;

        float totalGameSeconds = 360f;
        float elapsedSeconds = totalGameSeconds - remainingSeconds;

        // Which 30-second block are we currently in (0, 1, 2, 3...)
        int currentInterval = Mathf.FloorToInt(elapsedSeconds / 30f);

        // Only refresh the text when we cross into a new 30-second block
        if (currentInterval == lastDisplayedInterval) return;
        lastDisplayedInterval = currentInterval;

        float displaySeconds = currentInterval * 30f;
        float elapsedRatio = displaySeconds / totalGameSeconds;

        int inGameHour = Mathf.FloorToInt(elapsedRatio * 6f);
        int inGameMinuteProgress = Mathf.FloorToInt((elapsedRatio * 6f * 60f) % 60f);

        clockText.text = $"{inGameHour:00}:{inGameMinuteProgress:00} AM";

        Debug.Log($"[Clock] {displaySeconds}s elapsed -> {inGameHour:00}:{inGameMinuteProgress:00} AM");
    }

    private void HandleGameWon()
    {
        if (winPanel != null) winPanel.SetActive(true);
    }
    private void HandleGameLost()
    {
        if (losePanel != null) losePanel.SetActive(true);
    }
    private void HandleGamePaused()
    {
        if (pausePanel != null) pausePanel.SetActive(true);
    }
    private void HandleGameResumed()
    {
        if (pausePanel != null) pausePanel.SetActive(false);
    }
}