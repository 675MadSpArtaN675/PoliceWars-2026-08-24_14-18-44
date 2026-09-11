using System.Collections.Generic;

using UnityEngine;

public class DamageSystem : MonoBehaviour
{
    public delegate void OnDamage(GameObject from_, GameObject to_, int count_);
    public static DamageSystem Instance { get; private set; }

    private OnDamage _onDamage;
    public List<DamageNote> _damage_history { get; private set; }
    public TeamsRelationships Relationships;

    private void Awake()
    {
        _damage_history = new List<DamageNote>();

        if (Instance == null)
        {
            Instance = this;
        }
    }

    public void SetDamage(GameObject from_, GameObject to_, int count_)
    {
        if (from_ != null && to_ != null) {
            IDamagable damagable_component = to_.GetComponentInParent<IDamagable>();

            if (damagable_component != null)
            {
                Debug.Log($"Damaging from {from_.name} to {to_.name}. Damage count: {count_}.");
                damagable_component.SubHealth(count_);

                _onDamage?.Invoke(from_, to_, count_);

                _damage_history.Add(new DamageNote(from_, to_, count_));
            }
        }
    }
}

public struct DamageNote
{
    public readonly GameObject from;
    public readonly GameObject to;
    public readonly int count;

    public DamageNote(GameObject from_, GameObject to_, int count_)
    {
        from = from_;
        to = to_;
        count = count_;
    }
}