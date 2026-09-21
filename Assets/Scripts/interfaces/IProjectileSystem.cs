using UnityEngine;

public interface IProjectileSystem
{
    void SpawnProjectile(BulletSettings settings);
    void DestroyProjectile(GameObject projectile_object);
}