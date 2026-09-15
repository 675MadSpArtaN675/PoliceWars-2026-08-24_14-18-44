using UnityEngine;

public class DamagableEntity : MonoBehaviour, IDamagable
{
    public DamageSystem.OnDamageAction OnDamage;
    public DeathSystem.OnEntityDeathAction OnDeath;
    public DamageSystem.HealthChanged OnHealthChanged;

    public DeathSystem.OnEntityDeathAction OnDeathActions { get => OnDeath; }

    [SerializeField]
    [InspectorName("Health")]
    private int _health;

    [SerializeField]
    [InspectorName("Is Invulerable")]
    private bool _is_invulerable;

    private void Awake()
    {
        OnHealthChanged += UnitDeath;
    }

    public void SetHealth(int value)
    {
        int old_health = _health;
        _health = value;

        OnHealthChanged?.Invoke(this, old_health, _health);
    }

    public void AddHealth(int value)
    {
        int old_health = _health;
        _health += value;

        OnHealthChanged?.Invoke(this, old_health, _health);
    }

    public void SubHealth(int value)
    {
        int old_health = _health;
        _health -= value;

        OnHealthChanged?.Invoke(this, old_health, _health);
    }

    public void GiveDamage(GameObject who_damager, DamageSystem.DamageType damage_type, int damage_count)
    {
        SubHealth(damage_count);

        OnDamage?.Invoke(new DamageSystem.DamageNote(who_damager, gameObject, damage_type, damage_count));
    }

    public int GetHealth()
    {
        return _health;
    }

    public bool IsInvulerable()
    {
        return _is_invulerable;
    }

    private void UnitDeath(DamagableEntity entity, int old_health, int new_health)
    {
        Debug.Log($"Entity: {entity}, Object: {entity.gameObject}; Old health: {old_health}; Health: {new_health}");
        if (new_health <= 0)
        {
            DeathSystem.Instance?.DestroyUnit(entity.gameObject);
        }
    }
}