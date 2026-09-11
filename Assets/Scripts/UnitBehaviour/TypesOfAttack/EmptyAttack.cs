using UnityEngine;

public class EmptyAttack : AbstractAttack
{
    public EmptyAttack() : this(null, null) {}
    public EmptyAttack(UnitSettings settings, GameObject obj) : base(settings, obj)
    { }

    public override void Attack(in Vector2 _direction, int damage) {}
    public override Vector2[] CheckEnemy(ref bool _is_enemy_found)
    {
        return new Vector2[1];
    }
}