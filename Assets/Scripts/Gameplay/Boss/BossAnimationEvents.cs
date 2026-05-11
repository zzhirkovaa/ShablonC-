using UnityEngine;

public sealed class BossAnimationEvents : MonoBehaviour
{
    [SerializeField] private BossController _bossController;

    private void Awake()
    {
        if (_bossController == null)
            _bossController = GetComponentInParent<BossController>();
    }

    public void OnFireCastMoment()
    {
        _bossController?.OnFireCastMoment();
    }

    public void OnEarthStompMoment()
    {
        _bossController?.OnEarthStompMoment();
    }

    public void OnAirCastMoment()
    {
        _bossController?.OnAirCastMoment();
    }

    public void OnIceHitMoment()
    {
        _bossController?.OnIceHitMoment();
    }

    public void OnAttackFinished()
    {
        _bossController?.OnAttackAnimationFinished();
    }
}
