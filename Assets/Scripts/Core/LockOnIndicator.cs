using UnityEngine;

// NEW: floats a marker above the enemy you're locked onto, so you can see which one is targeted
// (especially useful now that you can switch targets).
//
// Setup: put this on any object (the player works), drag the player's LockOnController into the slot.
// With no Marker Prefab it uses a small white sphere as a placeholder. For a proper reticle,
// make a prefab (for example a Quad or Sprite with a reticle image) and drag it into Marker Prefab.
public class LockOnIndicator : MonoBehaviour
{
    [SerializeField] private LockOnController lockOn;
    [SerializeField] private GameObject markerPrefab; // optional: your own reticle (a prefab asset)

    [SerializeField] private float heightOffset = 0.6f;  // how far above the target's lock point
    [SerializeField] private float followSpeed = 20f;    // how quickly the marker slides to a new target

    private Transform marker;
    private Transform cam;

    private void Awake()
    {
        marker = markerPrefab != null
            ? Instantiate(markerPrefab).transform
            : CreatePlaceholder();

        marker.gameObject.SetActive(false);
    }

    private void LateUpdate()
    {
        Transform target = (lockOn != null && lockOn.IsLockedOn) ? lockOn.CurrentTarget : null;

        // Not locked on: hide the marker
        if (target == null)
        {
            if (marker.gameObject.activeSelf)
            {
                marker.gameObject.SetActive(false);
            }

            return;
        }

        Vector3 desired = target.position + Vector3.up * heightOffset;

        if (!marker.gameObject.activeSelf)
        {
            // Just locked on: appear right on the target
            marker.position = desired;
            marker.gameObject.SetActive(true);
        }
        else
        {
            // Switching targets: slide over smoothly
            float t = 1f - Mathf.Exp(-followSpeed * Time.unscaledDeltaTime);
            marker.position = Vector3.Lerp(marker.position, desired, t);
        }

        // Turn the marker to face the camera (a Quad or Sprite is visible from its back side, so this works for them)
        if (cam == null && Camera.main != null)
        {
            cam = Camera.main.transform;
        }

        if (cam != null)
        {
            Vector3 away = marker.position - cam.position;

            if (away.sqrMagnitude > 0.0001f)
            {
                marker.rotation = Quaternion.LookRotation(away);
            }
        }
    }

    private void OnDestroy()
    {
        if (marker != null)
        {
            Destroy(marker.gameObject);
        }
    }

    private Transform CreatePlaceholder()
    {
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.name = "LockOnMarker (placeholder)";
        sphere.transform.localScale = Vector3.one * 0.2f;

        // It's only a visual, so it must not collide with anything
        Destroy(sphere.GetComponent<Collider>());

        return sphere.transform;
    }
}