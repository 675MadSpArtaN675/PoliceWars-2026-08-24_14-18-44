using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Projectile: MonoBehaviour, IGameUnit
{
    [SerializeField]
    private Team _team;

    public Team Team { get => _team; set => _team = value; }

    [InspectorName("Damage")]
    public int Damage = 10;

    [InspectorName("Direction")]
    public Vector2 Direction = new Vector2(1, 0);

    private SpriteRenderer _sprite;
    private Transform _transform;
    private Mover _mover_comp;
    private bool _is_attacked = false;

    private void Awake()
    {
        _transform = GetComponent<Transform>();
        _mover_comp = GetComponent<Mover>();
        _sprite = GetComponentInChildren<SpriteRenderer>();
    }

    private void Start()
    {
        Debug.Log($"Bullet damage: {Damage}. Team: {Team}");
    }

    private void FixedUpdate()
    {
        if (!_is_attacked) {
            Attack(Damage, Direction);
        }
    }

    public void Attack(int damage, Vector2 direction)
    {
        float distance = _sprite.size.x * 0.75f;
        RaycastHit2D[] _check_dist = Physics2D.RaycastAll(_transform.position, direction, distance)
            .Where((RaycastHit2D _hit) => _hit.distance > 0 && _hit.distance < distance && _hit.collider.GetComponentInParent<ITeamable>() != null)
            .OrderBy((RaycastHit2D _hit) => _hit.distance)
            .ToArray();

        Debug.DrawRay(_transform.position, direction);

        if (_check_dist.Length > 0) {
            RaycastHit2D _hit = _check_dist.First();
            ITeamable teamable_component = _hit.collider.GetComponentInParent<ITeamable>();

            if (teamable_component != null)
            {
                Debug.Log($"Projectile team: {Team}. Enemy team: {teamable_component.Team}");
                List<Team> enemy_teams = DamageSystem.Instance.Relationships.GetEnemyTeams(Team);
                if (enemy_teams.Contains(teamable_component.Team)) {
                    DamageSystem.Instance?.SetDamage(gameObject, _hit.collider.gameObject, damage);
                    _mover_comp.MoveStop();

                    _is_attacked = true;
                    ProjectileSystem.Instance?.DestroyProjectile(gameObject);
                }
            }
        }
    }

    public IEnumerator Wait(float duration)
    {
        yield return new WaitForSecondsRealtime(duration);
    }
    public Vector2[] CheckEnemy()
    {
        return null;
    }

    public void Reset()
    {
        _is_attacked = false;
    }
}