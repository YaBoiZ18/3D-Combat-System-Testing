using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private InputReader input;
    [SerializeField] private Transform cameraRoot;
    [SerializeField] private Transform cameraTarget;

    [Header("Settings")]
    [SerializeField] private float sensitivity = 180f;
    [SerializeField] private float minPitch = -35f;
    [SerializeField] private float maxPitch = 70f;

    private float yaw;
    private float pitch;

    private float currentYaw;
    private float currentPitch;

    private float yawVelocity;
    private float pitchVelocity;

    [SerializeField] private float rotationSmoothTime = 0.08f;

    [SerializeField] private LockOnController lockOn;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        yaw = cameraRoot.eulerAngles.y;
        pitch = cameraTarget.localEulerAngles.x;

        // Convert pitch from 0-360 to -180 to 180
        if (pitch > 180f)
        {
            pitch -= 360f;
        }

        // Initialize current yaw and pitch to match the initial rotation
        currentYaw = yaw;
        currentPitch = pitch;

        // Lock the cursor to the center of the screen and hide it
        Cursor.lockState = CursorLockMode.Locked;

        // Hide the cursor
        Cursor.visible = false;
    }

    private void Update()
    {
        // Check if the Escape key is pressed to unlock the cursor
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // Unlock the cursor and make it visible
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        if (Input.GetMouseButtonDown(0))
        {
            // Lock the cursor and hide it when the left mouse button is clicked
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    private void LateUpdate()
    {
        if (lockOn.IsLockedOn)
        {
            // If locked on, rotate the camera to face the target
            RotateTowardsTarget();
        }
        else
        {
            // If not locked on, allow free rotation based on input
            RotateFree();
        }
    }

    private void RotateFree()
    {
        // Get the look input from the InputReader
        Vector2 look = input.LookInput;

        // Calculate the yaw and pitch based on the look input and sensitivity
        yaw += look.x * sensitivity;
        pitch -= look.y * sensitivity;

        // Clamp the pitch to prevent flipping
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        currentYaw = Mathf.SmoothDampAngle(
            currentYaw,
            yaw,
            ref yawVelocity,
            rotationSmoothTime
        );
        currentPitch = Mathf.SmoothDampAngle(
            currentPitch,
            pitch,
            ref pitchVelocity,
            rotationSmoothTime
        );

        // Apply the rotation to the camera root and target
        cameraRoot.rotation = Quaternion.Euler(0f, currentYaw, 0f);
        cameraTarget.localRotation = Quaternion.Euler(currentPitch, 0f, 0f);
    }

    // Rotate the camera to face the current lock-on target
    private void RotateTowardsTarget()
    {
        if (lockOn.CurrentTarget == null)
            return;

        // Calculate the direction from the camera root to the target
        Vector3 direction = lockOn.CurrentTarget.position - cameraRoot.position;

        // Create a rotation that looks in the direction of the target
        Quaternion targetRotation = Quaternion.LookRotation(direction);

        // Extract the yaw and pitch from the target rotation
        float targetYaw = targetRotation.eulerAngles.y;
        float targetPitch = targetRotation.eulerAngles.x;

        // Convert target pitch from 0-360 to -180 to 180
        if (targetPitch > 180f)
        {
            targetPitch -= 360f;
        }

        // Clamp the target pitch to prevent flipping
        targetPitch = Mathf.Clamp(targetPitch, minPitch, maxPitch);

        // Smoothly interpolate the current yaw and pitch towards the target values
        currentYaw = Mathf.SmoothDampAngle(
            currentYaw,
            targetYaw,
            ref yawVelocity,
            rotationSmoothTime
        );

        // Smoothly interpolate the current pitch towards the target pitch
        currentPitch = Mathf.SmoothDampAngle(
            currentPitch,
            targetPitch,
            ref pitchVelocity,
            rotationSmoothTime
        );

        // Apply the rotation to the camera root and target
        cameraRoot.rotation = Quaternion.Euler(0f, currentYaw, 0f);
        cameraTarget.localRotation = Quaternion.Euler(currentPitch, 0f, 0f);

        // Update the yaw and pitch to match the current values for the next frame
        yaw = currentYaw;
        pitch = currentPitch;
    }
}
