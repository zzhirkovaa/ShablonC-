using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "EnemyFactory",
    menuName = "Game/Enemies/Enemy Factory")]
public sealed class EnemyFactorySO : ScriptableObject
{
    [SerializeField] private GameObject _enemyPrefab;
    [SerializeField] private List<WeaponConfigSO> _availableWeapons = new();

    public GameObject EnemyPrefab => _enemyPrefab;
    public IReadOnlyList<WeaponConfigSO> AvailableWeapons => _availableWeapons;

    public GameObject Spawn(
        Vector3 position,
        Quaternion rotation,
        Transform playerTransform,
        RoomBounds roomBounds,
        bool isPeacefulMode)
    {
        if (_enemyPrefab == null)
        {
            Debug.LogWarning($"[{name}] Enemy prefab is not assigned.");
            return null;
        }

        GameObject enemyObject = Instantiate(_enemyPrefab, position, rotation);
        InitializeRuntime(enemyObject, playerTransform, roomBounds, isPeacefulMode);
        ApplyWeapon(enemyObject);
        return enemyObject;
    }

    private void InitializeRuntime(
        GameObject enemyObject,
        Transform playerTransform,
        RoomBounds roomBounds,
        bool isPeacefulMode)
    {
        if (enemyObject.TryGetComponent(out EnemySaveId saveId))
        {
            saveId.Unregister();
            Destroy(saveId);
        }

        if (enemyObject.TryGetComponent(out EnemyRoomReference roomReference))
            roomReference.SetRoomBounds(roomBounds);

        if (enemyObject.TryGetComponent(out EnemyAI enemyAI))
        {
            enemyAI.SetPeacefulMode(isPeacefulMode);
            enemyAI.Construct(playerTransform, roomBounds);
        }

        if (enemyObject.TryGetComponent(out EnemyRangedAI rangedAI))
        {
            rangedAI.SetPeacefulMode(isPeacefulMode);
            rangedAI.Construct(playerTransform, roomBounds);
        }

        if (enemyObject.TryGetComponent(out EnemyCombat meleeCombat))
            meleeCombat.Construct(playerTransform);

        if (enemyObject.TryGetComponent(out EnemyRangedCombat rangedCombat))
            rangedCombat.Construct(playerTransform);
    }

    private void ApplyWeapon(GameObject enemyObject)
    {
        WeaponConfigSO selectedWeapon = SelectWeapon();
        if (selectedWeapon == null)
            return;

        EnemyWeaponController weaponController =
            enemyObject.GetComponent<EnemyWeaponController>() ??
            enemyObject.AddComponent<EnemyWeaponController>();

        weaponController.InitializeWeapon(selectedWeapon);
    }

    private WeaponConfigSO SelectWeapon()
    {
        if (_availableWeapons == null || _availableWeapons.Count == 0)
        {
            Debug.LogWarning($"[{name}] Enemy factory has no weapon configs assigned.");
            return null;
        }

        List<WeaponConfigSO> validWeapons = new();
        foreach (WeaponConfigSO weapon in _availableWeapons)
        {
            if (weapon != null)
                validWeapons.Add(weapon);
        }

        if (validWeapons.Count == 0)
        {
            Debug.LogWarning($"[{name}] Enemy factory weapon list contains only null entries.");
            return null;
        }

        int index = Random.Range(0, validWeapons.Count);
        return validWeapons[index];
    }
}
