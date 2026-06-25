using UnityEngine;

public class InputReader : MonoBehaviour
{
    public Vector2 MoveInput { get; private set; }
    public bool SprintHeld { get; private set; }

    // Update is called once per frame
    void Update()
    {
        // Read the input from the player
        MoveInput = new Vector2(
            Input.GetAxisRaw("Horizontal"), 
            Input.GetAxisRaw("Vertical")
            );

        // Check if the sprint key is held down
        SprintHeld = Input.GetKey(KeyCode.LeftShift);
    }
}
