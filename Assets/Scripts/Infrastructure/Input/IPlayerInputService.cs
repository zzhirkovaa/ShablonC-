using UnityEngine;

public interface IPlayerInputService
{
    Vector2 GetMoveInput();
    bool IsRunPressed();
    bool IsPunchPressedThisFrame();
}