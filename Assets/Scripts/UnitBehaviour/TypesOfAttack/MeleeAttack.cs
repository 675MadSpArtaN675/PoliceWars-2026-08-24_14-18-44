using System.Collections.Generic;
using System.Linq;

using UnityEngine;

public class MeleeAttack : AbstractAttack
{
    protected override bool isEnableCaching => true;

    public MeleeAttack() : this(null, null) { }

    public MeleeAttack(UnitSettings settings, GameObject obj) : base(settings, obj)
    {}

    public override void Attack(in Vector2 _direction, int damage)
    {
        if (_unit_data.is_stop_when_attacking)
        {
            _mover.MoveStop();
        }

        AttackAction(_direction, damage);
    }

    protected virtual void AttackAction(in Vector2 _direction, int damage)
    {
        GameObject _nearest_obj = GetFromCache();

        if (_nearest_obj != null) {
            Debug.Log($"Nearest obj {_nearest_obj.name}");
            DamageSystem.Instance.SetDamage(_unit_game_obj, _nearest_obj, DamageSystem.DamageType.Hit, damage);

            ClearCache();
        }
    }

    public override Vector2[] CheckEnemy(ref bool _is_enemy_found)
    {
        Debug.Log($"Getted unit data: {_unit_data}");
        List<Vector2> _found_directions = new List<Vector2>();
        if (_unit_data.check_directions != null) {
            Debug.Log($"Checking directions. Dir count: {_unit_data.check_directions.Count}");
            foreach (Vector2 _direction in _unit_data.check_directions)
            {
                Vector3 unit_position = _unit_transformation.position;
                Debug.Log($"Check direction: {_direction}. Position: {unit_position}. Distance: {_unit_data.distance}");
                Vector2 direction_of_ray = new Vector2(unit_position.x, unit_position.y);
                Debug.DrawRay(direction_of_ray, _direction * _unit_data.distance, Color.green, 0.5f);

                RaycastHit2D[] scan_hit = Physics2D
                        .RaycastAll(direction_of_ray, _direction * _unit_data.distance, _unit_data.distance, 1 << _additional_settings.current_collide_id, 0, 4)
                        .Where((RaycastHit2D _hit) => IsValidEnemyTeam(in _hit))
                        .Where((RaycastHit2D _hit) => _hit.distance <= _unit_data.distance)
                        .ToArray();

                if (scan_hit.Count() > 0)
                {
                    GameObject[] obj = scan_hit.Select((_obj) => _obj.collider.gameObject)
                        .SelectMany((_obj) => _obj.GetComponentsInParent<IDamagable>())
                        .Select((_obj) => ((MonoBehaviour) _obj).gameObject)
                        .ToArray();

                    Debug.Log($"Enemy is found... Their count: {obj.Length}");

                    AddEntities(obj);

                    _found_directions.Add(_direction);
                    _is_enemy_found = true;
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


    protected bool IsValidEnemyTeam(in RaycastHit2D _hit)
    {
        ITeamable[] team_components = _hit.collider.GetComponentsInParent<ITeamable>()
            .Where((ITeamable team_comp) => team_comp.Team != _unit_team)
            .ToArray();

        if (team_components.Length > 0)
        {
            List<Team> teams = DamageSystem.Instance.Relationships?.GetEnemyTeams(_unit_team) ?? new List<Team>();
            Debug.Log($"Validating enemy team. Our team: {_unit_team}. Team count: {team_components.Length}. Enemy teams: {teams.Count}");

            if (teams.Count > 0) {
                Debug.Log($"Input team: {_unit_team}. Other teams({teams.Count}): {teams.Select((Team team) => team.ToString()).Aggregate((first, second) => first + ", " + second).TrimEnd(' ', ',')}");
                return true;
            }
        }

        return false;
    }
}