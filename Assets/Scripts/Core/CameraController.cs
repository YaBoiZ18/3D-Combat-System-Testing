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

    [SerializeField]
    private float rotationSmoothTime = 0.08f;

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

        currentYaw = yaw;
        currentPitch = pitch;
    }

    private void LateUpdate()
    {
        RotateCamera();
    }

    private void RotateCamera()
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
}
