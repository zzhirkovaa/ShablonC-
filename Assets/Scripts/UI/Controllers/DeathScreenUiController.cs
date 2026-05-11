using Player.UI;
using System;
using UnityEngine;

public sealed class DeathScreenUiController : IDisposable
{
    private readonly DeathScreenModel _model;
    private readonly IPlayerHealthModel _healthModel;
    private readonly DeathScreenView _view;
    private readonly ISceneLoader _sceneLoader;

    public DeathScreenUiController(
        DeathScreenModel model,
        IPlayerHealthModel healthModel,
        DeathScreenView view,
        ISceneLoader sceneLoader)
    {
        _model = model;
        _healthModel = healthModel;
        _view = view;
        _sceneLoader = sceneLoader;

        _healthModel.Died += OnDied;
        _view.RestartClicked += OnRestartClicked;

        _model.IsVisible = false;
        _view.HideImmediately();
    }

    public void Dispose()
    {
        _healthModel.Died -= OnDied;
        _view.RestartClicked -= OnRestartClicked;
    }

    private void OnDied()
    {
        _model.IsVisible = true;
        _view.ShowAfterDelay(_model.DelayBeforeShow);
    }

    private void OnRestartClicked()
    {
        _model.IsVisible = false;
        Time.timeScale = 1f;
        _sceneLoader.ReloadCurrent();
    }
}
