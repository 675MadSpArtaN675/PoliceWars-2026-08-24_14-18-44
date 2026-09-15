using UnityEditor;
using UnityEngine;

public interface IDamagable
{
    void SetHealth(int value);
    void AddHealth(int value);
    void SubHealth(int value);
    void GiveDamage(GameObject who_damager, DamageSystem.DamageType damage_type, int damage_count);

    int GetHealth();
    bool IsInvulerable();

    DeathSystem.OnEntityDeathAction OnDeathActions { get; }
}