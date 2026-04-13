using UnityEngine;

public sealed class RangedProjectileAttackLogic
{
    public void ShootAtTarget(Transform originTransform, Transform targetTransform, GameObject projectilePrefab, Transform firePoint)
    {
        if (targetTransform == null || projectilePrefab == null || firePoint == null)
            return;

        Vector3 lookAtTarget = new Vector3(targetTransform.position.x, originTransform.position.y, targetTransform.position.z);
        originTransform.LookAt(lookAtTarget);

        GameObject projectile = Object.Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
        Vector3 aimPoint = targetTransform.position + Vector3.up * 0.8f;
        Vector3 direction = (aimPoint - firePoint.position).normalized;
        projectile.transform.forward = direction;
    }
}
