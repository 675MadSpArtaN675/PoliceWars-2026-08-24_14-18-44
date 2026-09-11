using System;
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

        Reset();
    }

    private void FixedUpdate()
    {
        if (_speed != 0 && _body != null) {
            float distance = _speed * Time.deltaTime;
            float ray_distance = distance + _sprite_rend.size.x / 2;
            Vector2 speed = Direction * distance;

            RaycastHit2D[] _hits = Physics2D.RaycastAll(_body.position, Direction, ray_distance);
            RaycastHit2D? _hit = null;

            if (_hits.Length > 0) {
                foreach (RaycastHit2D hit in _hits)
                {
                    if (hit.distance > 0)
                    {
                        _hit = hit;
                        break;
                    }
                }
            }

            if (_hit.HasValue && isCollidable(in _hit) && CanStop)
            {
                Vector2 remains_distance_vec = _hit.Value.point - _body.position;
                float remains_distance = Mathf.Abs(Vector2.Distance(_hit.Value.point, _body.position));

                if (remains_distance <= distance)
                {
                    _body.position += remains_distance_vec;
                }
            }
            else {
                _body.position += speed;
            }
        }
    }

    private bool isCollidable(in RaycastHit2D? _hit)
    {
        if (_hit.HasValue && _hit.Value.collider != null)
        {
            Debug.Log($"Name: {_hit.Value.collider.gameObject.GetComponentInParent<ITeamable>()?.Team.ToString() ?? "None"}");
            ITeamable teamable_component = _hit.Value.collider.GetComponentInParent<ITeamable>();

            if (teamable_component != null)
            {
                List<Team> enemy_teams = DamageSystem.Instance.Relationships.GetEnemyTeams(_team);
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

    public void Reset()
    {
        _body = GetComponentInChildren<Rigidbody2D>();
        _speed = Speed;

        _team = GetComponentInParent<ITeamable>()?.Team ?? Team.NoTeam;

        Direction = UtilityGameFuncs.ClampVector(Direction, -1, 1);
    }
}