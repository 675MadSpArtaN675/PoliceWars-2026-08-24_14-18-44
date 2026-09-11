using UnityEngine;

public class DamagableEntity : MonoBehaviour, IDamagable
{
    public delegate void HealthChanged(DamagableEntity entity, int old_health, int new_health);

    public HealthChanged OnHealthChanged;

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

        if (OnHealthChanged != null) {
            OnHealthChanged.Invoke(this, old_health, _health);
        }
    }

    public void AddHealth(int value)
    {
        int old_health = _health;
        _health += value;

        if (OnHealthChanged != null) {
            OnHealthChanged.Invoke(this, old_health, _health);
        }
    }

    public void SubHealth(int value)
    {
        int old_health = _health;
        _health -= value;

        if (OnHealthChanged != null) {
            OnHealthChanged.Invoke(this, old_health, _health);
        }
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