using System.Collections.Generic;
using System.Linq;

using UnityEngine;

[CreateAssetMenu(fileName = "TestUnit", menuName = "Units/TestUnit")]
public class UnitSettings : ScriptableObject
{
    [InspectorName("Enemy check timeout")]
    public float check_timeout = 0.5f;

    [InspectorName("Distance to attack")]
    public float distance = 1;

    [InspectorName("Directions where check enemies")]
    public List<Vector2> check_directions = new List<Vector2>();

    [InspectorName("Timeout between attacks")]
    public float attack_timeout = 2;

    [InspectorName("Damage")]
    public int damage = 5;

    private void Awake()
    {
        Debug.Log($"Directions {check_directions}");
        check_directions = check_directions.Select((Vector2 not_normalized_vec) => UtilityGameFuncs.ClampVector(not_normalized_vec, -1, 1)).ToList();
    }
}
