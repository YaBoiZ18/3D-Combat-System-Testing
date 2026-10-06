using System.Collections;
using UnityEngine;

// NEW: a short freeze-frame when a hit lands. It's the cheapest way to make hits feel heavy.
// No setup needed: it creates itself the first time HitStop.Trigger() is called.
public class HitStop : MonoBehaviour
{
    private static HitStop instance;

    private Coroutine routine;
    private float savedTimeScale = 1f;

    // Call this from anywhere: HitStop.Trigger(0.06f);
    public static void Trigger(float duration, float slowScale = 0.05f)
    {
        if (duration <= 0f)
            return;

        if (instance == null)
        {
            instance = new GameObject("HitStop").AddComponent<HitStop>();
        }

        instance.Begin(duration, slowScale);
    }

    private void Begin(float duration, float slowScale)
    {
        if (routine != null)
        {
            // Already frozen: just restart the timer. Keep the ORIGINAL time scale.
            StopCoroutine(routine);
        }
        else
        {
            // Remember the normal speed so we put it back exactly (works with a pause menu too)
            savedTimeScale = Time.timeScale;
        }

        Time.timeScale = slowScale;
        routine = StartCoroutine(RestoreAfter(duration));
    }

    // Uses real time, because game time is what we just slowed down
    private IEnumerator RestoreAfter(float duration)
    {
        yield return new WaitForSecondsRealtime(duration);

        Time.timeScale = savedTimeScale;
        routine = null;
    }

    // Safety: never leave the game stuck in slow motion (e.g. on scene reload)
    private void OnDestroy()
    {
        if (routine != null)
        {
            Time.timeScale = savedTimeScale;
        }
    }
}