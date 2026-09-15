using System.Collections.Generic;

using UnityEngine;

public class DamageSystem : MonoBehaviour
{
    public struct DamageNote
    {
        public readonly GameObject from;
        public readonly GameObject to;
        public readonly DamageType damageType;
        public readonly int count;

        public DamageNote(GameObject from_, GameObject to_, DamageType damageType_, int count_)
        {
            from = from_;
            to = to_;
            count = count_;
            damageType = damageType_;
        }
    }

    public enum DamageType
    {
        NoType,
        Hit,
        Bullet,
        Fire,
        Explosion
    }

    public delegate void OnDamageAction(DamageNote damage);
    public delegate void HealthChanged(DamagableEntity entity, int old_health, int new_health);

    public static DamageSystem Instance { get; private set; }

    public OnDamageAction OnDamageForAllEntities;
    public List<DamageNote> damageHistory { get; private set; }
    public TeamsRelationships Relationships;

    private void Awake()
    {
        damageHistory = new List<DamageNote>();

        if (Instance == null)
        {
            Instance = this;
        }
    }

    public void SetDamage(GameObject from_, GameObject to_, DamageType damage_type, int count_)
    {
        if (from_ != null && to_ != null) {
            IDamagable damagable_component = to_.GetComponentInParent<IDamagable>();

            if (damagable_component != null)
            {
                DamageNote _note = new DamageNote(from_, to_, damage_type, count_);
                Debug.Log($"Damaging from {from_.name} to {to_.name}. Damage count: {count_}.");

                damagable_component.GiveDamage(from_, damage_type, count_);
                if (damagable_component.GetHealth() <= 0 && !damagable_component.IsInvulerable())
                {
                    DeathSystem.Instance.KillUnit(to_, _note);
                }

                OnDamageForAllEntities?.Invoke(_note);

                damageHistory.Add(_note);
            }
        }
    }
}
