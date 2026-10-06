using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private PlayerController playerController;

    [Header("Health UI")]
    [SerializeField] private Image healthFill;

    // NEW (optional): a second bar placed BEHIND healthFill (a red or white one works well).
    // After you take damage it drains slowly, so you can see how much health you just lost.
    // Leave it empty to skip the effect. It must be an Image with Image Type set to "Filled".
    [SerializeField] private Image healthTrailFill;
    [SerializeField] private float trailDelay = 0.4f;   // seconds before the trail starts draining
    [SerializeField] private float trailSpeed = 0.5f;   // how fast it drains (fraction of the bar per second)

    [Header("Guard UI")]
    [SerializeField] private Image guardFill;
    [SerializeField] private CanvasGroup guardCanvasGroup;

    [Header("Guard Fade Settings")]
    [SerializeField] private float fadeSpeed = 5f;
    [SerializeField] private float hideDelay = 1.5f;

    private float guardHideTimer;
    private float previousGuard;
    private float trailTimer;

    private void Start()
    {
        if (playerHealth != null)
        {
            previousGuard = playerHealth.CurrentGuard;
        }
    }

    void Update()
    {
        if (playerHealth == null)
            return;

        UpdateHealthBar();
        UpdateGuardBar();
        UpdateGuardVisibility();
    }

    private void UpdateHealthBar()
    {
        // CHANGED: null / zero checks so a missing reference doesn't throw errors every frame
        if (healthFill == null || playerHealth.MaxHealth <= 0f)
            return;

        float target = playerHealth.CurrentHealth / playerHealth.MaxHealth;

        // Took damage this frame: restart the trail's delay
        if (target < healthFill.fillAmount - 0.001f)
        {
            trailTimer = trailDelay;
        }

        healthFill.fillAmount = target;

        if (healthTrailFill == null)
            return;

        // Healing: the trail follows right away
        if (target >= healthTrailFill.fillAmount)
        {
            healthTrailFill.fillAmount = target;
            return;
        }

        // Wait a moment, then drain down to the real health
        if (trailTimer > 0f)
        {
            trailTimer -= Time.unscaledDeltaTime;
            return;
        }

        healthTrailFill.fillAmount = Mathf.MoveTowards(
            healthTrailFill.fillAmount,
            target,
            trailSpeed * Time.unscaledDeltaTime
        );
    }

    private void UpdateGuardBar()
    {
        if (guardFill == null || playerHealth.MaxGuard <= 0f)
            return;

        guardFill.fillAmount = playerHealth.CurrentGuard / playerHealth.MaxGuard;
    }

    // Updates the visibility of the guard UI based on blocking state and guard changes.
    private void UpdateGuardVisibility()
    {
        if (guardCanvasGroup == null)
            return;

        bool isBlocking = playerController != null &&
                          playerController.IsBlocking;

        bool guardChanged =
            !Mathf.Approximately(
                playerHealth.CurrentGuard,
                previousGuard
            );

        if (isBlocking || guardChanged)
        {
            guardHideTimer = hideDelay;
        }
        else if (guardHideTimer > 0f)
        {
            // CHANGED: unscaled time, so the UI doesn't slow down during hit stop
            guardHideTimer -= Time.unscaledDeltaTime;
        }

        float targetAlpha = (isBlocking || guardHideTimer > 0f) ? 1f : 0f;

        guardCanvasGroup.alpha = Mathf.MoveTowards(
            guardCanvasGroup.alpha,
            targetAlpha,
            fadeSpeed * Time.unscaledDeltaTime
        );

        previousGuard = playerHealth.CurrentGuard;
    }
}