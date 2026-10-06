using UnityEngine;

// NEW: runs before every other script, so they all read THIS frame's input
// instead of sometimes getting last frame's, depending on Unity's script order.
[DefaultExecutionOrder(-100)]
public class InputReader : MonoBehaviour
{
    public Vector2 MoveInput { get; private set; }
    public bool SprintHeld { get; private set; }
    public bool LockPressed { get; private set; }

    public Vector2 LookInput { get; private set; }

    public bool CombatPressed { get; private set; }
    public bool AttackPressed { get; private set; }

    public bool DodgePressed { get; private set; }

    public bool BlockHeld { get; private set; }

    // NEW: mouse wheel (positive = scrolled up). Used to switch lock-on targets.
    public float ScrollInput { get; private set; }

    void Update()
    {
        // NEW: true while the cursor is hidden/locked (i.e. you're playing, not in a menu)
        bool cursorLocked = Cursor.lockState == CursorLockMode.Locked;

        // CHANGED: ClampMagnitude so diagonals aren't 1.41x stronger than straight movement.
        // This also keeps the animator's MoveX/MoveY values inside the blend tree's range.
        MoveInput = Vector2.ClampMagnitude(
            new Vector2(
                Input.GetAxisRaw("Horizontal"),
                Input.GetAxisRaw("Vertical")
            ),
            1f
        );

        // CHANGED: no camera movement while the cursor is released (after pressing Escape)
        LookInput = cursorLocked
            ? new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"))
            : Vector2.zero;

        SprintHeld = Input.GetKey(KeyCode.LeftShift);

        LockPressed = Input.GetMouseButtonDown(2);

        CombatPressed = Input.GetKeyDown(KeyCode.Q);

        // CHANGED: the click that re-locks the cursor no longer also counts as an attack
        AttackPressed = cursorLocked && Input.GetMouseButtonDown(0);

        DodgePressed = Input.GetKeyDown(KeyCode.Space);

        BlockHeld = Input.GetMouseButton(1);

        // NEW: ignored while the cursor is released, same as the look input
        ScrollInput = cursorLocked ? Input.mouseScrollDelta.y : 0f;
    }
}