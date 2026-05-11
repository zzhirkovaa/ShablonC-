using UnityEngine;

[DisallowMultipleComponent]
public sealed class EnemyWeaponController : MonoBehaviour
{
    private WeaponConfigSO _currentWeapon;
    private GameObject _spawnedVisual;

    public WeaponConfigSO CurrentWeapon => _currentWeapon;

    public void InitializeWeapon(WeaponConfigSO weaponConfig)
    {
        if (weaponConfig == null)
        {
            Debug.LogWarning($"[{name}] Enemy weapon config is not assigned.");
            return;
        }

        _currentWeapon = weaponConfig;

        switch (weaponConfig)
        {
            case MeleeWeaponConfigSO meleeWeapon:
                ApplyMeleeWeapon(meleeWeapon);
                break;

            case RangedWeaponConfigSO rangedWeapon:
                ApplyRangedWeapon(rangedWeapon);
                break;

            default:
                Debug.LogWarning($"[{name}] Unsupported weapon config type: {weaponConfig.GetType().Name}");
                break;
        }
    }

    private void ApplyMeleeWeapon(MeleeWeaponConfigSO weaponConfig)
    {
        if (TryGetComponent(out EnemyCombat combat))
            combat.ApplyWeaponConfig(weaponConfig);
        else
            Debug.LogWarning($"[{name}] Melee weapon '{weaponConfig.WeaponName}' requires EnemyCombat.");

        if (TryGetComponent(out EnemyStatefulAIBase statefulAi))
            statefulAi.OverrideAttackRange(weaponConfig.AttackRange);

        AttachVisual(weaponConfig);
        UpdateNameSuffix(weaponConfig.WeaponName);
    }

    private void ApplyRangedWeapon(RangedWeaponConfigSO weaponConfig)
    {
        if (_spawnedVisual != null)
        {
            Destroy(_spawnedVisual);
            _spawnedVisual = null;
        }

        if (TryGetComponent(out EnemyRangedCombat combat))
            combat.ApplyWeaponConfig(weaponConfig);
        else
            Debug.LogWarning($"[{name}] Ranged weapon '{weaponConfig.WeaponName}' requires EnemyRangedCombat.");

        if (TryGetComponent(out EnemyStatefulAIBase statefulAi))
            statefulAi.OverrideAttackRange(weaponConfig.AttackRange);

        UpdateNameSuffix(weaponConfig.WeaponName);
    }

    private void AttachVisual(MeleeWeaponConfigSO weaponConfig)
    {
        if (_spawnedVisual != null)
        {
            Destroy(_spawnedVisual);
            _spawnedVisual = null;
        }

        if (weaponConfig.WeaponVisualPrefab == null)
            return;

        Transform anchor = ResolveAnchor(weaponConfig);
        if (anchor == null)
        {
            Debug.LogWarning($"[{name}] Could not resolve anchor '{weaponConfig.WeaponAnchorName}' or fallback bone '{weaponConfig.FallbackBoneName}'.");
            return;
        }

        _spawnedVisual = Instantiate(weaponConfig.WeaponVisualPrefab, anchor);
        _spawnedVisual.transform.localPosition = weaponConfig.LocalPosition;
        _spawnedVisual.transform.localRotation = Quaternion.Euler(weaponConfig.LocalRotationEuler);
        _spawnedVisual.transform.localScale = weaponConfig.LocalScale;
    }

    private Transform ResolveAnchor(MeleeWeaponConfigSO weaponConfig)
    {
        Transform anchor = TransformSearchUtility.FindChildRecursive(transform, weaponConfig.WeaponAnchorName);
        if (anchor != null)
            return anchor;

        Transform fallbackBone = TransformSearchUtility.FindChildRecursive(transform, weaponConfig.FallbackBoneName);
        if (fallbackBone == null)
            return null;

        GameObject anchorObject = new GameObject(weaponConfig.WeaponAnchorName);
        anchorObject.transform.SetParent(fallbackBone, false);
        return anchorObject.transform;
    }

    private void UpdateNameSuffix(string weaponName)
    {
        if (string.IsNullOrWhiteSpace(weaponName))
            return;

        int separatorIndex = gameObject.name.IndexOf(" [", System.StringComparison.Ordinal);
        string baseName = separatorIndex >= 0 ? gameObject.name[..separatorIndex] : gameObject.name;
        gameObject.name = $"{baseName} [{weaponName}]";
    }
}
