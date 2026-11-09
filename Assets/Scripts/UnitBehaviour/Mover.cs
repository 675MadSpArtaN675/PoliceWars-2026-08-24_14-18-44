using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

struct ChainMoveElement
{
    public Vector3 positionToMove;
    public Action<Mover> actionOnEnd;

    public ChainMoveElement(Vector3 _positionToMove, Action<Mover> _actionEnd)
    {
        positionToMove = _positionToMove;
        actionOnEnd = _actionEnd;
    }
}

public class Mover : MonoBehaviour
{
    private const float MIN_DISTANCE_TO_POINT_FOR_TP = 0.01f;

    public float Speed;
    public bool CanStop = true;
    public bool CanChooseInMove = false;
    public Vector2 Direction;
    public PlantSystem.CollideLineLayer collideLineLayer = PlantSystem.CollideLineLayer.BankLayer;

    private Vector2 _direction;

    private Team _team;
    private float _speed;

    private Rigidbody2D _body;
    private SpriteRenderer _sprite_rend;
    private ChoosableUnit _choosable_unit_component;

    private Vector3? position_to_move;
    private Stack<ChainMoveElement> _positions_chain;
    private Action<Mover> _on_move_stopped;
    private bool _is_can_choose;

    private void Start()
    {
        _sprite_rend = GetComponentInChildren<SpriteRenderer>();
        _positions_chain = new Stack<ChainMoveElement>();
        _choosable_unit_component = GetComponentInChildren<ChoosableUnit>(true);

        ResetMove();
    }

    private void FixedUpdate()
    {

        if (_speed != 0 && _body != null) {
            float distance = _speed * Time.deltaTime;
            float ray_distance = distance + _sprite_rend.size.x / 2;
            Vector2 speed = _direction * distance;

            RaycastHit2D? _hit = SearchNearestUnit(ray_distance);

            if (_hit.HasValue && isCollidable(_hit.Value.collider))
            {
                Vector2 remains_distance_vec = _hit.Value.point - _body.position;
                float remains_distance = Mathf.Abs(Vector2.Distance(_hit.Value.point, _body.position));

                if (remains_distance <= distance && isCanMoveToPoint())
                {
                    _body.position += remains_distance_vec;
                    if (_choosable_unit_component != null)
                    {
                        _choosable_unit_component.IsCanBeChoose = false;
                    }
                }
                else if (_choosable_unit_component != null)
                {
                    _choosable_unit_component.IsCanBeChoose = true;
                }
            }
            else if (isCanMoveToPoint())
            {
                _body.position += speed;
                if (_choosable_unit_component != null)
                {
                    _choosable_unit_component.IsCanBeChoose = false;
                }
            }
            else
            {
                transform.position = position_to_move.Value;
                MoveStop();
            }
        }
    }

    public void MoveTo(Vector2 position, bool is_cancel_other = false, Action<Mover> on_move_end = null)
    {
        Vector3 final_pos = position;
        Vector3 direction = Vector3.Normalize(final_pos - transform.position);

        Debug.Log($"MT. Calculated direction: {direction}. End position: {final_pos}");
        if (is_cancel_other)
        {
            _positions_chain.Clear();
        }

        ChainMoveElement _move_element;
        if ((_positions_chain.TryPeek(out _move_element) || position_to_move.HasValue) && !is_cancel_other) {
            _positions_chain.Push(new ChainMoveElement(final_pos, on_move_end));
        }
        else
        {
            _direction = direction;
            position_to_move = final_pos;
        }

        if (_choosable_unit_component != null)
        {
            _choosable_unit_component.IsCanBeChoose = false;
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

    protected bool isCanMoveToPoint()
    {
        if (position_to_move.HasValue)
        {
            float distance = calculateDistanceToPoint(position_to_move.Value);
            Debug.Log($"MT. pos: {position_to_move.Value}. Distance: {distance}. TP: {position_to_move.HasValue && distance > MIN_DISTANCE_TO_POINT_FOR_TP}");
        }
        return !position_to_move.HasValue || (position_to_move.HasValue && calculateDistanceToPoint(position_to_move.Value) > MIN_DISTANCE_TO_POINT_FOR_TP);
    }

    protected float calculateDistanceToPoint(Vector3 input_position)
    {
        return Mathf.Abs(Vector3.Distance(transform.position, input_position));
    }

    protected RaycastHit2D? SearchNearestUnit(float ray_distance)
    {
        RaycastHit2D[] _hits;
        if (collideLineLayer != PlantSystem.CollideLineLayer.BankLayer)
        {
            int layer_mask = 1 << LayerMask.NameToLayer(collideLineLayer.ToString());
            _hits = Physics2D.RaycastAll(_body.position, _direction, ray_distance, layer_mask);
        }
        else
        {
            _hits = Physics2D.RaycastAll(_body.position, _direction, ray_distance);
        }

        _hits = _hits
            .Where((RaycastHit2D _hit) => Mathf.Abs(_hit.distance) > 0 && Mathf.Abs(_hit.distance) <= ray_distance && GetComponentInParent<ITeamable>() != null)
            .OrderBy((RaycastHit2D _hit) => _hit.distance)
            .ToArray();

        RaycastHit2D? _hit = null;
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
        _on_move_stopped?.Invoke(this);
        _on_move_stopped = null;

        if (_choosable_unit_component != null)
        {
            _choosable_unit_component.IsCanBeChoose = true;
        }

        Debug.Log($"MT 1. Position: {position_to_move}.");
        StartCoroutine(setPosWait(0.2f));
    }

    protected IEnumerator setPosWait(float seconds_between_move)
    {
        yield return new WaitForSecondsRealtime(seconds_between_move);

        ChainMoveElement _element;
        if (_positions_chain.TryPop(out _element))
        {
            Vector3 _pos = position_to_move.Value;

            _direction = Vector3.Normalize(_element.positionToMove - _pos);
            position_to_move = _element.positionToMove;

            Debug.Log($"MT 2. Position: {position_to_move}.");
            if (_element.actionOnEnd != null) {
                _on_move_stopped += _element.actionOnEnd;
            }

            _speed = Speed;
        }
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

        _direction = UtilityGameFuncs.ClampVector(Direction, -1, 1);
        position_to_move = null;

        if (_choosable_unit_component != null)
        {
            _choosable_unit_component.IsCanBeChoose = false;
        }
    }
}