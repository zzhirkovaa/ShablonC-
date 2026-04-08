using System;
using UnityEngine;

public sealed class PauseMenuInputListener : MonoBehaviour, IPauseMenuInput
{
    [SerializeField] private KeyCode _toggleKey = KeyCode.Escape;

    public event Action ToggleRequested;

    private void Update()
    {
        if (Input.GetKeyDown(_toggleKey))
            ToggleRequested?.Invoke();
    }
}
