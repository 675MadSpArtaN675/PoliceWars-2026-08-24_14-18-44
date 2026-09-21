using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

[CreateAssetMenu(fileName = "UnitSettings", menuName = "Units/StandardUnitSettings")]
public class UnitSettings : ScriptableObject
{
    private List<TypeOfAttack> _types_who_need_projectile = new List<TypeOfAttack>() { TypeOfAttack.Ranged, TypeOfAttack.MeleeAndRanged };
    private Dictionary<int, float> subattacks_timeout;

    [InspectorName("Health")]
    public int health = 100;

    [InspectorName("Damage")]
    public int damage = 5;

    [InspectorName("Move speed")]
    public float speed = 0;

    [InspectorName("Attack Type")]
    public TypeOfAttack attack_type = TypeOfAttack.Empty;

    [InspectorName("Projectile object")]
    public GameObject projectile = null;

    [InspectorName("Start timeout")]
    public float start_timeout = 4;

    [InspectorName("Timeout between full attacks")]
    public float attack_timeout = 2;

    [InspectorName("Timeout after subattack number N")]
    public List<SubattackTimeout> subattack_timeout = new List<SubattackTimeout>();

    [InspectorName("Distance to attack")]
    public float distance = 1;

    [InspectorName("Directions where check enemies")]
    public List<Vector2> check_directions = new List<Vector2>();

    [InspectorName("Is stop moving when unit attacking (If unit is movable)")]
    public bool is_stop_when_attacking = false;

    private void Awake()
    {
        if (subattacks_timeout == null && subattack_timeout.Count > 0)
        {
            subattacks_timeout = subattack_timeout
                .Select((SubattackTimeout timeout_par) => (timeout_par.AttackNum, timeout_par.WaitInSeconds))
                .ToDictionary(pair => pair.Item1, pair => pair.Item2);
        }
        else
        {
            subattacks_timeout = new Dictionary<int, float>();
        }

        Debug.Log($"Directions: {check_directions}");
        check_directions = check_directions.Select((Vector2 not_normalized_vec) => UtilityGameFuncs.ClampVector(not_normalized_vec, -1, 1)).ToList();

        if (projectile?.GetComponent<Projectile>() == null && _types_who_need_projectile.Contains(attack_type))
        {
            throw new NullReferenceException("No projectile type");
        }
    }

    public float GetSubAttackTimeout(int subattack_num)
    {
        if (subattacks_timeout?.ContainsKey(subattack_num) ?? false)
        {
            return subattacks_timeout[subattack_num];
        }

        return 0.0f;
    }
}

[Serializable]
public struct SubattackTimeout
{
    public int AttackNum;
    public float WaitInSeconds;
}