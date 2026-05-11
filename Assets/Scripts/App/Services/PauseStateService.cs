using UnityEngine;

public sealed class PauseStateService : IPauseStateService
{
    private readonly MonoBehaviour[] _scriptsToDisableOnPause;

    public PauseStateService(MonoBehaviour[] scriptsToDisableOnPause)
    {
        _scriptsToDisableOnPause = scriptsToDisableOnPause;
    }

    public void EnterPause()
    {
        Time.timeScale = 0f;
        SetGameplayScriptsEnabled(false);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ExitPause()
    {
        Time.timeScale = 1f;
        SetGameplayScriptsEnabled(true);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void SetGameplayScriptsEnabled(bool enabledState)
    {
        if (_scriptsToDisableOnPause == null)
            return;

        foreach (MonoBehaviour script in _scriptsToDisableOnPause)
        {
            if (script != null)
                script.enabled = enabledState;
        }
    }
}
