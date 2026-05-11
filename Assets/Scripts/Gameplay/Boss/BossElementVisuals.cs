using UnityEngine;

public sealed class BossElementVisuals : MonoBehaviour
{
    [Header("Points")]
    [SerializeField] private Transform _firePoint;
    [SerializeField] private Transform _earthFootPoint;
    [SerializeField] private Transform _airPoint;
    [SerializeField] private Transform _leftHandPoint;
    [SerializeField] private Transform _rightHandPoint;

    private GameObject _activeAirEffect;

    public Transform FirePoint => _firePoint != null ? _firePoint : transform;
    public Transform EarthFootPoint => _earthFootPoint != null ? _earthFootPoint : transform;
    public Transform AirPoint => _airPoint != null ? _airPoint : transform;
    public Transform LeftHandPoint => _leftHandPoint != null ? _leftHandPoint : transform;
    public Transform RightHandPoint => _rightHandPoint != null ? _rightHandPoint : transform;

    public void SpawnEarthEffect(BossElementConfig config)
    {
        SpawnAt(config != null ? config.EarthEffectPrefab : null, EarthFootPoint, config);
    }

    public void SpawnAirEffect(BossElementConfig config)
    {
        ClearAirEffect();
        _activeAirEffect = SpawnDetachedAt(config != null ? config.AirEffectPrefab : null, AirPoint);
        FreezePhysics(_activeAirEffect);
        AnchorInWorld(_activeAirEffect, AirPoint);
    }

    public void ClearAirEffect()
    {
        if (_activeAirEffect == null)
            return;

        Destroy(_activeAirEffect);
        _activeAirEffect = null;
    }

    private void OnDisable()
    {
        ClearAirEffect();
    }

    public void SpawnIceEffects(BossElementConfig config)
    {
        if (config == null)
            return;

        SpawnAt(config.IceLeftHandEffectPrefab, LeftHandPoint, config);
        SpawnAt(config.IceRightHandEffectPrefab, RightHandPoint, config);
    }

    private void SpawnAt(GameObject prefab, Transform point, BossElementConfig config)
    {
        if (prefab == null || point == null)
            return;

        GameObject instance = Instantiate(prefab, point.position, point.rotation, point);
        Destroy(instance, config != null ? Mathf.Max(0.1f, config.EffectLifetime) : 2f);
    }

    private GameObject SpawnDetachedAt(GameObject prefab, Transform point)
    {
        if (prefab == null || point == null)
            return null;

        return Instantiate(prefab, point.position, point.rotation);
    }

    private void FreezePhysics(GameObject instance)
    {
        if (instance == null)
            return;

        Rigidbody[] rigidbodies = instance.GetComponentsInChildren<Rigidbody>();
        foreach (Rigidbody body in rigidbodies)
        {
            body.linearVelocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
            body.isKinematic = true;
        }
    }

    private void AnchorInWorld(GameObject instance, Transform point)
    {
        if (instance == null || point == null)
            return;

        BossStaticWorldEffect anchor = instance.GetComponent<BossStaticWorldEffect>();
        if (anchor == null)
            anchor = instance.AddComponent<BossStaticWorldEffect>();

        anchor.Initialize(point.position, point.rotation);
    }
}
