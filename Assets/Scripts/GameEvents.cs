using System;

/// <summary>
/// Pure C# static event bus. No MonoBehaviour, no ScriptableObject, no GameManager reference needed.
/// Any script can call GameEvents.TriggerXxx(...) to broadcast, and any script can subscribe
/// in OnEnable() / unsubscribe in OnDisable().
/// </summary>
public static class GameEvents
{
    // 
    // State Broadcasts (GameManager -> Listeners)
    // 
    public static event Action<int> OnHeartsChanged;       // Current hearts (0-3)
    public static event Action<int> OnDreamChanged;        // Current dream index (1-4)
    public static event Action<float> OnClockUpdated;      // Remaining seconds (360 -> 0)
    public static event Action OnGameWon;                  // Win trigger at 6:00 AM
    public static event Action OnGameLost;                 // Death trigger
    public static event Action OnGameStarted;
    public static event Action OnGamePaused;
    public static event Action OnGameResumed;


    // 
    // Action Requests (Entities -> GameManager)
    // 
    public static event Action<int> OnRequestTakeDamage;   // Fired by enemy on player contact
    public static event Action OnRequestRestoreHeart;      // Fired by Rewind Minigame success

    // 
    // Feature Triggers
    // 
    public static event Action<UnityEngine.Vector2> OnPlayerPositionChanged;
    public static event Action OnWeaponDepleted;
    public static event Action OnRewindAvailable;
    public static event Action OnRewindUsed;

    // 
    // Trigger methods — call these instead of invoking the events directly.
    // (C# only lets the declaring class invoke an event, so these wrappers
    // are what every other script actually calls.)
    // 

    // State Broadcasts
    public static void TriggerHeartsChanged(int hearts) => OnHeartsChanged?.Invoke(hearts);
    public static void TriggerDreamChanged(int dreamIndex) => OnDreamChanged?.Invoke(dreamIndex);
    public static void TriggerClockUpdated(float remainingSeconds) => OnClockUpdated?.Invoke(remainingSeconds);
    public static void TriggerGameWon() => OnGameWon?.Invoke();
    public static void TriggerGameLost() => OnGameLost?.Invoke();
    public static void TriggerGameStarted() => OnGameStarted?.Invoke();
    public static void TriggerGamePaused() => OnGamePaused?.Invoke();
    public static void TriggerGameResumed() => OnGameResumed?.Invoke();

    // Action Requests
    public static void TriggerRequestTakeDamage(int amount) => OnRequestTakeDamage?.Invoke(amount);
    public static void TriggerRequestRestoreHeart() => OnRequestRestoreHeart?.Invoke();

    // Feature Triggers
    public static void TriggerPlayerPositionChanged(UnityEngine.Vector2 position) => OnPlayerPositionChanged?.Invoke(position);
    public static void TriggerWeaponDepleted() => OnWeaponDepleted?.Invoke();
    public static void TriggerRewindAvailable() => OnRewindAvailable?.Invoke();
    public static void TriggerRewindUsed() => OnRewindUsed?.Invoke();

    // 
    // Optional but recommended for a jam: clears all subscribers on scene reload.
    // Prevents "ghost" listeners from a previous play session (very common
    // bug source when testing repeatedly in the Editor).
    // 
    public static void ClearAllListeners()
    {
        OnHeartsChanged = null;
        OnDreamChanged = null;
        OnClockUpdated = null;
        OnGameWon = null;
        OnGameLost = null;
        OnRequestTakeDamage = null;
        OnRequestRestoreHeart = null;
        OnPlayerPositionChanged = null;
        OnWeaponDepleted = null;
        OnRewindAvailable = null;
        OnRewindUsed = null;
    }
}