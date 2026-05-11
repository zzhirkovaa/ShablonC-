using UnityEngine;

[CreateAssetMenu(
    fileName = "MeleeWeaponConfig",
    menuName = "Game/Weapons/Melee Weapon Config")]
public sealed class MeleeWeaponConfigSO : WeaponConfigSO
{
    [Header("Visual")]
    [SerializeField] private GameObject _weaponVisualPrefab;
    [SerializeField] private string _weaponAnchorName = "WeaponAnchor_L";
    [SerializeField] private string _fallbackBoneName = "hand.L";
    [SerializeField] private Vector3 _localPosition = Vector3.zero;
    [SerializeField] private Vector3 _localRotationEuler = Vector3.zero;
    [SerializeField] private Vector3 _localScale = Vector3.one;

    public GameObject WeaponVisualPrefab => _weaponVisualPrefab;
    public string WeaponAnchorName => _weaponAnchorName;
    public string FallbackBoneName => _fallbackBoneName;
    public Vector3 LocalPosition => _localPosition;
    public Vector3 LocalRotationEuler => _localRotationEuler;
    public Vector3 LocalScale => _localScale;
}
