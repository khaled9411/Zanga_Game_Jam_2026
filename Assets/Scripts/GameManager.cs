using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("Hearts")]
    [SerializeField] private int maxHearts = 3;
    private int currentHearts;

    [Header("Dream / Clock")]
    [SerializeField] private int totalDreams = 4;
    private int currentDream = 1;

    [SerializeField] private float totalGameSeconds = 360f;
    private float remainingSeconds;
    private float secondsPerDream;

    [Header("Death Delay")]
    [SerializeField] private float deathScreenDelay = 4f;


    private bool gameStarted = false;
    private bool gameEnded = false;
    private bool isPaused = false;

    private float lowHeartTimer = 0f;
    private bool rewindHasBeenUsedThisRun = false;
    private const float REWIND_SURVIVE_TIME = 5f;

    public int CurrentHearts => currentHearts;
    public int CurrentDream => currentDream;
    public float RemainingSeconds => remainingSeconds;
    public bool IsGameEnded => gameEnded;
    public bool IsPaused => isPaused;

    private bool rewindWindowOpen = false; // prevents re-firing every frame

    private void OnEnable()
    {
        GameEvents.OnRequestTakeDamage += HandleRequestTakeDamage;
        GameEvents.OnRequestRestoreHeart += HandleRequestRestoreHeart;
        GameEvents.OnGameStarted += HandleGameStarted;
    }

    private void OnDisable()
    {
        GameEvents.OnRequestTakeDamage -= HandleRequestTakeDamage;
        GameEvents.OnRequestRestoreHeart -= HandleRequestRestoreHeart;
        GameEvents.OnGameStarted -= HandleGameStarted;
    }

    private void Start()
    {
        // Auto-start the run as soon as this scene (SampleScene) loads
        StartGame();
    }

    private void HandleGameStarted()
    {
        CancelInvoke(nameof(ShowLoseScreen));
        rewindWindowOpen = false;
        currentHearts = maxHearts;
        currentDream = 1;
        remainingSeconds = totalGameSeconds;
        secondsPerDream = totalGameSeconds / totalDreams;
        gameEnded = false;
        gameStarted = true;
        isPaused = false;
        Time.timeScale = 1f;

        lowHeartTimer = 0f;
        rewindHasBeenUsedThisRun = false;

        GameEvents.TriggerHeartsChanged(currentHearts);
        GameEvents.TriggerDreamChanged(currentDream);
        GameEvents.TriggerClockUpdated(remainingSeconds);
    }

    private void Update()
    {
        if (!gameStarted || gameEnded) return;

        if (Input.GetKeyDown(KeyCode.Escape))
            TogglePause();

        if (isPaused) return;

        TickClock();
        TickRewindWindow();
    }

    private void TogglePause()
    {
        isPaused = !isPaused;
        Time.timeScale = isPaused ? 0f : 1f;

        if (isPaused)
            GameEvents.TriggerGamePaused();
        else
            GameEvents.TriggerGameResumed();
    }

    // Call this from a Resume button too, if you want a button instead of only Escape
    public void ResumeGame()
    {
        if (!isPaused) return;
        TogglePause();
    }

    private void TickClock()
    {
        remainingSeconds -= Time.deltaTime;
        if (remainingSeconds < 0f) remainingSeconds = 0f;

        GameEvents.TriggerClockUpdated(remainingSeconds);

        float elapsed = totalGameSeconds - remainingSeconds;
        int expectedDream = Mathf.Clamp(Mathf.FloorToInt(elapsed / secondsPerDream) + 1, 1, totalDreams);
        if (expectedDream != currentDream)
        {
            currentDream = expectedDream;
            GameEvents.TriggerDreamChanged(currentDream);
        }

        if (remainingSeconds <= 0f)
        {
            WinGame();
        }
    }

    private void TickRewindWindow()
    {
        if (rewindHasBeenUsedThisRun || rewindWindowOpen) return;

        if (currentHearts != 1)
        {
            lowHeartTimer = 0f;
            return;
        }

        lowHeartTimer += Time.deltaTime;
        if (lowHeartTimer >= REWIND_SURVIVE_TIME)
        {
            rewindWindowOpen = true;
            GameEvents.TriggerRewindAvailable();
            Debug.Log("[GameManager] Rewind window opened — pickup should spawn once now");
        }
    }

    private void HandleRequestTakeDamage(int amount)
    {
        if (!gameStarted || gameEnded || isPaused) return;

        currentHearts = Mathf.Max(0, currentHearts - amount);
        GameEvents.TriggerHeartsChanged(currentHearts);

        if (currentHearts != 1)
        {
            lowHeartTimer = 0f;
        }

        if (currentHearts <= 0)
        {
            LoseGame();
        }
    }

    private void HandleRequestRestoreHeart()
    {
        rewindWindowOpen = false;
        if (!gameStarted || gameEnded || isPaused) return;

        currentHearts = Mathf.Min(maxHearts, currentHearts + 1);
        GameEvents.TriggerHeartsChanged(currentHearts);
        GameEvents.TriggerRewindUsed();

        rewindHasBeenUsedThisRun = true;
        lowHeartTimer = 0f;
    }

    private void WinGame()
    {
        if (gameEnded) return;
        gameEnded = true;
        gameStarted = false;
        Time.timeScale = 1f;
        GameEvents.TriggerGameWon();
    }

    private void LoseGame()
    {
        if (gameEnded) return;
        gameEnded = true;
        gameStarted = false;
        Time.timeScale = 1f;

        Debug.Log($"Player died — lose screen in {deathScreenDelay}s");
        Invoke(nameof(ShowLoseScreen), deathScreenDelay);
    }

    private void ShowLoseScreen()
    {
        GameEvents.TriggerGameLost();
    }

    public void StartGame()
    {
        GameEvents.TriggerGameStarted();
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("SampleScene");
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    public void ExitGame()
    {
        Time.timeScale = 1f;
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}