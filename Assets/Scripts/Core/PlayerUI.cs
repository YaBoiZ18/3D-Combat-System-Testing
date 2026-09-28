using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private PlayerController playerController;

    [Header("Health UI")]
    [SerializeField] private Image healthFill;

    [Header("Guard UI")]
    [SerializeField] private Image guardFill;
    [SerializeField] private CanvasGroup guardCanvasGroup;

    [Header("Guard Fade Settings")]
    [SerializeField] private float fadeSpeed = 5f;
    [SerializeField] private float hideDelay = 1.5f;

    private float guardHideTimer;
    private float previousGuard;

    private void Start()
    {
        if (playerHealth != null)
        {
            previousGuard = playerHealth.CurrentGuard;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(playerHealth == null)
            return;

        // Update health bar
        UpdateHealthBar();
        UpdateGuardBar();
        UpdateGuardVisibility();
    }

    private void UpdateHealthBar()
    {
        healthFill.fillAmount = playerHealth.CurrentHealth / playerHealth.MaxHealth;

    }

    private void UpdateGuardBar()
    {
        guardFill.fillAmount = playerHealth.CurrentGuard / playerHealth.MaxGuard;
    }

    private void UpdateGuardVisibility()
    {
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
            guardHideTimer -= Time.deltaTime;
        }

        float targetAlpha;

        if (isBlocking || guardHideTimer > 0f)
        {
            targetAlpha = 1f;
        }
        else
        {
            targetAlpha = 0f;
        }

        guardCanvasGroup.alpha = Mathf.MoveTowards(
            guardCanvasGroup.alpha,
            targetAlpha,
            fadeSpeed * Time.deltaTime
        );

        previousGuard = playerHealth.CurrentGuard;
    }
}
