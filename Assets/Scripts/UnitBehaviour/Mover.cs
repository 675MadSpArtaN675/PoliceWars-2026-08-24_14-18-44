using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Mover : MonoBehaviour
{
    public float Speed;
    public bool CanStop = true;
    public Vector2 Direction;

    private Team _team;
    private float _speed;

    private Rigidbody2D _body;
    private SpriteRenderer _sprite_rend;

    private void Start()
    {
        _sprite_rend = GetComponentInChildren<SpriteRenderer>();

        ResetMove();
    }

    private void FixedUpdate()
    {
        if (_speed != 0 && _body != null) {
            float distance = _speed * Time.deltaTime;
            float ray_distance = distance + _sprite_rend.size.x / 2;
            Vector2 speed = Direction * distance;

            RaycastHit2D? _hit = SearchNearestUnit(ray_distance);

            if (_hit.HasValue && isCollidable(_hit.Value.collider))
            {
                Vector2 remains_distance_vec = _hit.Value.point - _body.position;
                float remains_distance = Mathf.Abs(Vector2.Distance(_hit.Value.point, _body.position));

                if (remains_distance <= distance)
                {
                    _body.position += remains_distance_vec;
                }
            }
            else
            {
                _body.position += speed;
            }
        }
    }


    public bool IsStopped()
    {
        return _speed == 0;
    }

    public bool IsNotMovable()
    {
        return Speed == 0 && _speed == 0;
    }

    protected RaycastHit2D? SearchNearestUnit(float ray_distance)
    {
        RaycastHit2D? _hit = null;
        RaycastHit2D[] _hits = Physics2D.RaycastAll(_body.position, Direction, ray_distance)
            .Where((RaycastHit2D _hit) => _hit.distance > 0 && GetComponentInParent<ITeamable>() != null)
            .OrderBy((RaycastHit2D _hit) => _hit.distance)
            .ToArray();

        if (_hits.Length > 0)
        {
            _hit = _hits.First();
        }

        return _hit;
    }

    protected bool isCollidable(Collider2D _hit)
    {
        if (_hit != null)
        {
            Debug.Log($"Name: {_hit.gameObject.GetComponentInParent<ITeamable>()?.Team.ToString() ?? "None"}");
            ITeamable teamable_component = _hit.GetComponentInParent<ITeamable>();

            if (teamable_component != null)
            {
                List<Team> enemy_teams = DamageSystem.Instance?.Relationships.GetEnemyTeams(_team) ?? new List<Team>();
                Debug.Log($"Team: {_team} Enemy teams: {enemy_teams}");
                if (teamable_component is Projectile)
                {
                    return false;
                }
                else if (enemy_teams != null && enemy_teams.Contains(teamable_component.Team))
                {
                    return true;
                }
            }
        }

        return false;
    }

    public void MoveStop()
    {
        _speed = 0;
    }

    public void ResumeMove()
    {
        _speed = Speed;
    }

    public void ResetMove()
    {
        _body = GetComponentInChildren<Rigidbody2D>();
        _speed = Speed;

        _team = GetComponentInParent<ITeamable>()?.Team ?? Team.NoTeam;

        Direction = UtilityGameFuncs.ClampVector(Direction, -1, 1);
    }
}