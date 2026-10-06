using UnityEngine;
using UnityEngine.SceneManagement;

// NEW: fades in a "you died" panel after the player dies, then restarts the scene on a key press.
//
// Setup:
//  1. Put this script on an object that is ALWAYS active (for example the same Canvas as PlayerUI).
//  2. Make a panel with a CanvasGroup (a dark background plus your "YOU DIED / Press R to restart" text).
//  3. Drag the player's PlayerHealth and the panel's CanvasGroup into the slots below.
public class DeathScreen : MonoBehaviour
{
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private CanvasGroup screen;

    [Header("Timing")]
    [SerializeField] private float showDelay = 1.5f;   // seconds after dying before the screen starts to appear
    [SerializeField] private float fadeSpeed = 1f;     // 1 = fades in over one second

    [Header("Restart")]
    [SerializeField] private KeyCode restartKey = KeyCode.R;

    // Tracks whether the player has died and we should start showing the screen.
    private bool playerDied;
    // Countdown before the death screen starts to fade in.
    private float delayTimer;

    private void Start()
    {
        // Start hidden, and don't block clicks on the UI underneath
        if (screen != null)
        {
            screen.alpha = 0f;
            screen.blocksRaycasts = false;
            screen.interactable = false;
        }
    }

    private void OnEnable()
    {
        // Subscribe to the player's death event so we can trigger the UI.
        if (playerHealth != null)
        {
            playerHealth.Died += OnPlayerDied;
        }
    }

    private void OnDisable()
    {
        // Unsubscribe to avoid dangling event handlers / memory leaks.
        if (playerHealth != null)
        {
            playerHealth.Died -= OnPlayerDied;
        }
    }

    private void OnPlayerDied()
    {
        // Event handler called when the player's health signals death.
        playerDied = true;
        delayTimer = showDelay;
    }

    private void Update()
    {
        if (!playerDied || screen == null)
            return;

        // Unscaled time, because the game briefly goes into slow motion when the player dies
        if (delayTimer > 0f)
        {
            delayTimer -= Time.unscaledDeltaTime;
            return;
        }

        screen.alpha = Mathf.MoveTowards(screen.alpha, 1f, fadeSpeed * Time.unscaledDeltaTime);

        if (screen.alpha >= 1f && Input.GetKeyDown(restartKey))
        {
            Restart();
        }
    }

    private void Restart()
    {
        // Reloading the scene resets the player, the enemies and everything else.
        // If Unity says the scene isn't in the build, add it via File > Build Settings > Add Open Scenes.
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}