using UnityEngine;
using Player.Interfaces;
using Player.Core;

/// <summary>
///  онтроллер дл€ смены внешности (глаза)
/// </summary>
public class AppearanceController : MonoBehaviour
{
    [Header("Debug Keys")]
    [SerializeField] private KeyCode _normalStateKey = KeyCode.Alpha1;
    [SerializeField] private KeyCode _angryStateKey = KeyCode.Alpha2;
    [SerializeField] private KeyCode _happyStateKey = KeyCode.Alpha3;
    [SerializeField] private KeyCode _deadStateKey = KeyCode.Alpha4;

    private IPlayerAppearance _appearance;
    private PlayerController _playerController;

    private void Awake()
    {
        _playerController = GetComponent<PlayerController>();

        _appearance = new PlayerAppearance(
            GetComponent<Animator>(),
            GetComponentsInChildren<Renderer>(),
            0.2f
        );
    }

    private void Update()
    {
        if (Input.GetKeyDown(_normalStateKey))
            _appearance.SetEyeState("normal");

        if (Input.GetKeyDown(_angryStateKey))
            _appearance.SetEyeState("angry");

        if (Input.GetKeyDown(_happyStateKey))
            _appearance.SetEyeState("happy");

        if (Input.GetKeyDown(_deadStateKey))
            _appearance.SetEyeState("dead");
    }
}