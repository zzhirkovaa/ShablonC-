using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public sealed class PlayerStatusEffects : MonoBehaviour
{
    [SerializeField] private PlayerController _playerController;
    [SerializeField] private PlayerHealth _playerHealth;
    [SerializeField] private CharacterController _characterController;

    private Coroutine _controlRoutine;
    private Coroutine _burnRoutine;
    private Coroutine _spinRoutine;
    private float _controlDisabledUntil;

    private void Awake()
    {
        if (_playerController == null)
            _playerController = GetComponent<PlayerController>();

        if (_playerHealth == null)
            _playerHealth = GetComponent<PlayerHealth>();

        if (_characterController == null)
            _characterController = GetComponent<CharacterController>();
    }

    public void DisableControl(float seconds)
    {
        if (seconds <= 0f || _playerController == null)
            return;

        _controlDisabledUntil = Mathf.Max(_controlDisabledUntil, Time.time + seconds);
        _playerController.enabled = false;

        if (_controlRoutine == null)
            _controlRoutine = StartCoroutine(ControlRoutine());
    }

    public void Freeze(float seconds)
    {
        DisableControl(seconds);
    }

    public void KnockUp(float height)
    {
        StartCoroutine(KnockUpRoutine(height));
    }

    public void KnockUp(float height, float disableControlSeconds)
    {
        if (disableControlSeconds > 0f)
            DisableControl(disableControlSeconds);

        KnockUp(height);
    }

    public void ApplyBurnDamage(float damagePerSecond, float duration, GameObject source = null)
    {
        if (_burnRoutine != null)
            StopCoroutine(_burnRoutine);

        _burnRoutine = StartCoroutine(BurnRoutine(damagePerSecond, duration, source));
    }

    public void PullTo(Vector3 targetPosition, float duration)
    {
        PullTo(targetPosition, duration, 0f);
    }

    public void PullTo(Vector3 targetPosition, float duration, float speed)
    {
        StartCoroutine(PullRoutine(targetPosition, duration, speed));
    }

    public void SpinAroundY(float duration, float degrees)
    {
        if (_spinRoutine != null)
            StopCoroutine(_spinRoutine);

        _spinRoutine = StartCoroutine(SpinAroundYRoutine(duration, degrees));
    }

    private IEnumerator ControlRoutine()
    {
        while (Time.time < _controlDisabledUntil)
            yield return null;

        if (_playerController != null && (_playerHealth == null || _playerHealth.CurrentHealth > 0f))
            _playerController.enabled = true;

        _controlRoutine = null;
    }

    private IEnumerator BurnRoutine(float damagePerSecond, float duration, GameObject source)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            float deltaTime = Time.deltaTime;
            elapsed += deltaTime;

            if (_playerHealth != null && damagePerSecond > 0f)
                _playerHealth.TakeDamage(new DamageInfo(damagePerSecond * deltaTime, DamageType.Magical, source));

            yield return null;
        }

        _burnRoutine = null;
    }

    private IEnumerator KnockUpRoutine(float height)
    {
        if (_characterController == null || height <= 0f)
            yield break;

        const float knockUpDuration = 0.45f;
        float elapsed = 0f;
        float speed = height / knockUpDuration;

        while (elapsed < knockUpDuration)
        {
            float deltaTime = Time.deltaTime;
            elapsed += deltaTime;
            _characterController.Move(Vector3.up * (speed * deltaTime));
            yield return null;
        }
    }

    private IEnumerator PullRoutine(Vector3 targetPosition, float duration, float speed)
    {
        if (_characterController == null || duration <= 0f)
            yield break;

        DisableControl(duration);

        float elapsed = 0f;
        while (elapsed < duration)
        {
            float deltaTime = Time.deltaTime;
            elapsed += deltaTime;

            Vector3 current = transform.position;
            Vector3 next = speed > 0f
                ? Vector3.MoveTowards(current, targetPosition, speed * deltaTime)
                : Vector3.Lerp(current, targetPosition, Mathf.Clamp01(elapsed / duration));

            _characterController.Move(next - current);
            yield return null;
        }
    }

    private IEnumerator SpinAroundYRoutine(float duration, float degrees)
    {
        if (duration <= 0f || Mathf.Approximately(degrees, 0f))
        {
            _spinRoutine = null;
            yield break;
        }

        float elapsed = 0f;
        float rotated = 0f;

        while (elapsed < duration)
        {
            float deltaTime = Time.deltaTime;
            elapsed += deltaTime;

            float targetRotated = Mathf.Lerp(0f, degrees, Mathf.Clamp01(elapsed / duration));
            float deltaDegrees = targetRotated - rotated;
            rotated = targetRotated;

            transform.Rotate(Vector3.up, deltaDegrees, Space.World);
            yield return null;
        }

        _spinRoutine = null;
    }
}
