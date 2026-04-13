using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class EnemyHealth : MonoBehaviour, IDamageable, IHealth
{
    [Header("Settings")]
    public float maxHealth = 50f;
    [SerializeField] private float _despawnDelay = 3f;

    public float CurrentHealth => _state.CurrentHealth;
    public float MaxHealth => _state.MaxHealth;
    public bool IsDead => _state != null && _state.IsDead;

    public event Action<float> OnHealthChanged;

    private Animator _animator;
    private EnemyHealthState _state;
    private Coroutine _despawnCoroutine;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _state = new EnemyHealthState(maxHealth);
        _state.HealthChanged += RaiseHealthChanged;
        _state.Died += Die;
    }

    private void Start()
    {
        _state.PublishCurrentState();
    }

    private void OnDestroy()
    {
        if (_state == null)
            return;

        _state.HealthChanged -= RaiseHealthChanged;
        _state.Died -= Die;
    }

    public void TakeDamage(DamageInfo damage)
    {
        if (!_state.ApplyDamage(damage.Amount))
            return;

        Debug.Log($"{gameObject.name} получил {damage.Type} урон. Осталось ХП: {_state.CurrentHealth}");
    }

    public void RestoreState(float currentHealth, bool isDead)
    {
        if (_state == null)
            return;

        CancelDespawn();

        if (!gameObject.activeSelf)
            gameObject.SetActive(true);

        _state.Restore(currentHealth, isDead);

        if (_state.IsDead)
        {
            ApplyDeadPresentation();
            gameObject.SetActive(false);
            return;
        }

        ApplyAlivePresentation();
    }

    private void RaiseHealthChanged(float healthPercentage)
    {
        OnHealthChanged?.Invoke(healthPercentage);
    }

    private void Die()
    {
        ApplyDeadPresentation();
        ScheduleDespawn();
    }

    private void ApplyDeadPresentation()
    {
        SetRuntimeComponentsEnabled(false);

        if (_animator != null)
        {
            _animator.SetBool("IsRunning", false);
            _animator.SetFloat("Speed", 0f);
            _animator.SetTrigger("Die");
        }
    }

    private void ApplyAlivePresentation()
    {
        SetRuntimeComponentsEnabled(true);

        if (_animator != null)
        {
            _animator.Rebind();
            _animator.Update(0f);
            _animator.SetBool("IsRunning", false);
            _animator.SetFloat("Speed", 0f);
        }
    }

    private void SetRuntimeComponentsEnabled(bool enabled)
    {
        if (TryGetComponent<EnemyAI>(out var meleeAi))
            meleeAi.enabled = enabled;

        if (TryGetComponent<EnemyRangedAI>(out var rangedAi))
            rangedAi.enabled = enabled;

        if (TryGetComponent<EnemyCombat>(out var meleeCombat))
            meleeCombat.enabled = enabled;

        if (TryGetComponent<EnemyRangedCombat>(out var rangedCombat))
            rangedCombat.enabled = enabled;

        if (TryGetComponent<NavMeshAgent>(out var agent))
        {
            agent.enabled = enabled;
            if (enabled)
                agent.isStopped = false;
        }

        if (TryGetComponent<Collider>(out var col))
            col.enabled = enabled;
    }

    private void ScheduleDespawn()
    {
        CancelDespawn();
        _despawnCoroutine = StartCoroutine(DisableAfterDelay());
    }

    private void CancelDespawn()
    {
        if (_despawnCoroutine == null)
            return;

        StopCoroutine(_despawnCoroutine);
        _despawnCoroutine = null;
    }

    private IEnumerator DisableAfterDelay()
    {
        yield return new WaitForSeconds(_despawnDelay);
        _despawnCoroutine = null;
        gameObject.SetActive(false);
    }
}
