using UnityEngine;

public class GunnerAttack : MeleeAttack
{
    protected override bool isEnableCaching => false;

    public GunnerAttack() : base() {}

    public GunnerAttack(UnitSettings settings, GameObject obj) : base(settings, obj)
    {
        DisableFoundEntityCache();
    }

    protected override void AttackAction(in Vector2 _direction, int damage)
    {
        Vector3 unit_transform = _unit_transformation?.position ?? Vector3.zero;
        Debug.Log($"Attacking by direction: {_direction}. Position of unit: {unit_transform}");

        ProjectileSystem.Instance.SpawnProjectile(new BulletSettings { Velocity = 10, Direction = _direction, OwnersTeam = _unit_team, AdditionalSettings = _additional_settings, SpawnPosition = new Vector2(unit_transform.x, unit_transform.y), Damage = damage});
    }
}
