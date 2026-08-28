using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;


public class GunnerBehaviour : MonoBehaviour
{
    [SerializeField]
    private UnitSettings _unit_data;

    private Transform _unit_transformation;
    private bool _is_attacking = false;
    private bool _is_enemy_found;

    private void Awake()
    {
        _unit_transformation = GetComponent<Transform>();

        if (_unit_data != null) {
            Debug.Log($"Created unit data: {_unit_data}. Directions: {_unit_data.check_directions.Count}");

            foreach (Vector2 _direction in _unit_data.check_directions)
            {
                Debug.Log($"Found direction: {_direction}...");
            }
        }
    }

    private void FixedUpdate()
    {
        if (!_is_attacking) {
            StartCoroutine(AttackCycle());
        }
    }

    public IEnumerator AttackCycle()
    {
        _is_attacking = true;
        yield return new WaitForSeconds(2);

        Vector2[] _directions_to_attack = CheckEnemy();

        if (_is_enemy_found)
        {
            foreach (Vector2 _direction in _directions_to_attack)
            {
                Attack(_unit_data.damage, _direction);
                yield return new WaitForSeconds(2);
            }
        }

        _is_attacking = false;
    }

    public void Attack(int damage, Vector2 _direction)
    {
        Debug.Log($"Attacking by direction: {_direction}");
    }

    public Vector2[] CheckEnemy()
    {
        Debug.Log($"Getted unit data: {_unit_data}");
        List<Vector2> _found_directions = new List<Vector2>();
        if (_unit_data.check_directions != null) {
            Debug.Log("Checking directions");
            foreach (Vector2 _direction in _unit_data.check_directions)
            {
                Vector3 unit_position = _unit_transformation.position;
                Debug.Log($"Check direction: {_direction}. Position: {unit_position}. Distance: {_unit_data.distance}");
                Vector2 direction_of_ray = new Vector2(unit_position.x, unit_position.y);

                RaycastHit2D[] scan_hit = Physics2D
                        .RaycastAll(direction_of_ray, _direction * _unit_data.distance, _unit_data.distance)
                        .Where((RaycastHit2D _hit) => _hit.collider.GetComponentsInParent<Mover>().Count() > 0 )
                        .ToArray();

                Debug.DrawRay(unit_position, _direction * _unit_data.distance, Color.red, 5);
                if (scan_hit.Count() > 0)
                {
                    Debug.Log("Enemy is found...");
                    _is_enemy_found = true;

                    _found_directions.Add(_direction);
                }
                else
                {
                    Debug.Log("Enemy is not found...");
                }
            }
        }

        Debug.Log("Checking is finished");
        return _found_directions.ToArray();
    }

    public void Reset()
    {
        _is_enemy_found = false;
    }
}
