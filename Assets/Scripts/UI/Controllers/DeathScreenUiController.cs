using Player.UI;
using System;
using UnityEngine;

public sealed class DeathScreenUiController : IDisposable
{
    private readonly IPlayerHealthModel _healthModel;
    private readonly DeathScreenView _view;
    private readonly ISceneLoader _sceneLoader;
    private readonly float _delayBeforeShow;

    public DeathScreenUiController(
        IPlayerHealthModel healthModel,
        DeathScreenView view,
        ISceneLoader sceneLoader,
        float delayBeforeShow = 2.5f)
    {
        _healthModel = healthModel;
        _view = view;
        _sceneLoader = sceneLoader;
        _delayBeforeShow = delayBeforeShow;

        _healthModel.Died += OnDied;
        _view.RestartClicked += OnRestartClicked;

        _view.HideImmediately();
    }

    public void Dispose()
    {
        _healthModel.Died -= OnDied;
        _view.RestartClicked -= OnRestartClicked;
    }

    private void OnDied()
    {
        _view.ShowAfterDelay(_delayBeforeShow);
    }

    private void OnRestartClicked()
    {
        Time.timeScale = 1f;
        _sceneLoader.ReloadCurrent();
    }
}