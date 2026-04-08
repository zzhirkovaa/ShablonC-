using UnityEngine;

// MVC Model: stores the player's runtime input state without Unity scene logic.
public sealed class PlayerModel
{
    public Vector2 MoveInput { get; private set; }
    public bool IsRunning { get; private set; }
    public bool IsMoving { get; private set; }
    public float InputMagnitude { get; private set; }

    public void UpdateInput(Vector2 moveInput, bool isRunning, float moveThreshold)
    {
        MoveInput = moveInput;
        IsRunning = isRunning;
        InputMagnitude = moveInput.magnitude;
        IsMoving = InputMagnitude > moveThreshold;
    }
}
