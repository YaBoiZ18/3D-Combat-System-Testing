using UnityEngine;

public class InputReader : MonoBehaviour
{
    public Vector2 MoveInput { get; private set; }
    public bool SprintHeld { get; private set; }
    public bool LockPressed { get; private set; }

    public Vector2 LookInput { get; private set; }

    public bool CombatPressed { get; private set; }
    public bool AttackPressed { get; private set; }

    public bool DodgePressed { get; private set; }

    // Update is called once per frame
    void Update()
    {
        // Read the input from the player
        MoveInput = new Vector2(
            Input.GetAxisRaw("Horizontal"), 
            Input.GetAxisRaw("Vertical")
            );

        LookInput = new Vector2(
             Input.GetAxisRaw("Mouse X"),
             Input.GetAxisRaw("Mouse Y")
            );

        // Check if the sprint key is held down
        SprintHeld = Input.GetKey(KeyCode.LeftShift);

        // Check if the lock key is pressed
        LockPressed = Input.GetMouseButtonDown(2);

        // Check if the combat key is pressed
        CombatPressed = Input.GetKeyDown(KeyCode.Q);

        // Check if the attack key is pressed
        AttackPressed = Input.GetMouseButtonDown(0);

        // Check if the dodge key is pressed
        DodgePressed = Input.GetKeyDown(KeyCode.Space);
    }
}
