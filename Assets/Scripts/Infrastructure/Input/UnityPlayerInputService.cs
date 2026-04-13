using UnityEngine;

public sealed class UnityPlayerInputService : IPlayerInputService
{
    public Vector2 GetMoveInput()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        return new Vector2(horizontal, vertical);
    }

    public bool IsRunPressed()
    {
        return Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
    }

    public bool IsPunchPressedThisFrame()
    {
        return Input.GetMouseButtonDown(0);
    }
}