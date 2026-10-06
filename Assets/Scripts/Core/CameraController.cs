using UnityEngine;

//  Handles player camera behavior for both free-look and lock-on modes.
// Smoothly interpolates yaw/pitch and applies them to camera root/target.
public class CameraController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private InputReader input;
    [SerializeField] private Transform cameraRoot;
    [SerializeField] private Transform cameraTarget;
    [SerializeField] private LockOnController lockOn;

    [Header("Free Look")]
    [SerializeField] private float sensitivity = 180f;
    [SerializeField] private float minPitch = -35f;
    [SerializeField] private float maxPitch = 70f;
    [SerializeField] private float rotationSmoothTime = 0.08f;

    // NEW: lock-on gets its own settings so it can feel different from free look
    [Header("Lock-On")]
    [SerializeField] private float lockOnSmoothTime = 0.15f; // slower = less snapping when you lock on
    [SerializeField] private float lockOnMinPitch = -10f;    // limits how far up the camera tilts
    [SerializeField] private float lockOnMaxPitch = 35f;     // limits how far down the camera tilts

    // Raw target angles driven by input or lock-on
    private float yaw;
    private float pitch;

    // Smoothed/applied angles
    private float currentYaw;
    private float currentPitch;

    // Velocities used by SmoothDampAngle
    private float yawVelocity;
    private float pitchVelocity;

    void Start()
    {
        // Initialize yaw/pitch from current transforms so smoothing starts from the visible camera state.
        yaw = cameraRoot.eulerAngles.y;
        pitch = cameraTarget.localEulerAngles.x;

        // Convert pitch from 0-360 to -180 to 180
        if (pitch > 180f)
        {
            pitch -= 360f;
        }

        currentYaw = yaw;
        currentPitch = pitch;

        // Lock and hide the cursor for gameplay by default.
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        // Toggle cursor visibility/lock with Escape and re-lock on left-click.
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        if (Input.GetMouseButtonDown(0))
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    private void LateUpdate()
    {
        // CHANGED: null check so the camera still works if lockOn isn't assigned
        if (lockOn != null && lockOn.IsLockedOn)
        {
            RotateTowardsTarget();
        }
        else
        {
            RotateFree();
        }
    }

    private void RotateFree()
    {
        // Read raw look input and convert to angle deltas.
        Vector2 look = input.LookInput;

        yaw += look.x * sensitivity;
        pitch -= look.y * sensitivity;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        // CHANGED: the smoothing code lives in one helper now (see SmoothRotate below)
        SmoothRotate(yaw, pitch, rotationSmoothTime);
    }

    private void RotateTowardsTarget()
    {
        if (lockOn.CurrentTarget == null)
            return;

        // CHANGED: aim from the camera pivot (cameraTarget) instead of cameraRoot.
        // cameraRoot sits low (around the player's feet), which made the pitch look too far upward.
        // Compute the direction vector from the camera pivot to the lock-on target.
        Vector3 direction = lockOn.CurrentTarget.position - cameraTarget.position;

        // NEW: LookRotation logs a warning if the direction is zero
        if (direction.sqrMagnitude < 0.01f)
            return;

        Quaternion targetRotation = Quaternion.LookRotation(direction);

        float targetYaw = targetRotation.eulerAngles.y;
        float targetPitch = targetRotation.eulerAngles.x;

        if (targetPitch > 180f)
        {
            targetPitch -= 360f;
        }

        // CHANGED: uses the tighter lock-on pitch limits
        targetPitch = Mathf.Clamp(targetPitch, lockOnMinPitch, lockOnMaxPitch);

        SmoothRotate(targetYaw, targetPitch, lockOnSmoothTime);

        // Keep free-look values in sync so there's no jump when you unlock
        yaw = currentYaw;
        pitch = currentPitch;
    }

    // NEW: replaces the duplicated SmoothDamp + apply-rotation code
    private void SmoothRotate(float targetYaw, float targetPitch, float smoothTime)
    {
        // Smoothly approach the target angles and apply to transforms.
        currentYaw = Mathf.SmoothDampAngle(currentYaw, targetYaw, ref yawVelocity, smoothTime);
        currentPitch = Mathf.SmoothDampAngle(currentPitch, targetPitch, ref pitchVelocity, smoothTime);

        cameraRoot.rotation = Quaternion.Euler(0f, currentYaw, 0f);
        cameraTarget.localRotation = Quaternion.Euler(currentPitch, 0f, 0f);
    }
}