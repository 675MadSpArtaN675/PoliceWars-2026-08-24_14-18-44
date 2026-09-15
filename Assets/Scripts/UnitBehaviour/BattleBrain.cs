using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


public class BattleBrain : MonoBehaviour, IGameUnit
{
    [SerializeField]
    private Team _team;
    public Team Team { get {
            return _team;
        }
        set {
            _team = value;
        }
    }

    [SerializeField]
    private PlantSystem.CollideLineLayer colliderLayer = PlantSystem.CollideLineLayer.BankLayer;

    public PlantSystem.CollideLineLayer ColliderLayer { get => colliderLayer; set => colliderLayer = value; }
    public PlantSystem.UnitAdditionalSettings additionalSettings { get; set; }

    [SerializeField]
    private UnitSettings _unit_data;

    private AbstractAttack _attack_type;

    private bool _is_attacking = true;
    private bool _is_enemy_found = false;
    private bool _ready_to_actions = false;

    private void Awake()
    {
        if (_unit_data != null) {
            Debug.Log($"Created unit data: {_unit_data}. Directions: {_unit_data.check_directions.Count}");

            foreach (Vector2 _direction in _unit_data.check_directions)
            {
                Debug.Log($"Found direction: {_direction}...");
            }
        }
    }

    private void Start()
    {
        SetOptionsByColliderLayer(ColliderLayer);
        InitAttackType(_unit_data.attack_type);
    }

    public void SetOptionsByColliderLayer(PlantSystem.CollideLineLayer colliderLine)
    {
        additionalSettings = PlantSystem.UnitAdditionalSettings.CreateInstance(colliderLine);
        PlantSystem.UnitAdditionalSettings.SetMasksForComponents(gameObject, additionalSettings);
    }

    public void SetOptions(PlantSystem.UnitAdditionalSettings colliderLine)
    {
        additionalSettings = colliderLine;
        PlantSystem.UnitAdditionalSettings.SetMasksForComponents(gameObject, additionalSettings);
    }

    private void InitAttackType(TypeOfAttack _attack)
    {
        switch (_attack)
        {
            case TypeOfAttack.Empty:
                _attack_type = AbstractAttack.CreateEmpty(_unit_data, gameObject);
                break;

            case TypeOfAttack.Melee:
                _attack_type = AbstractAttack.CreateAttackType<MeleeAttack>(_unit_data, gameObject);
                break;

            case TypeOfAttack.Ranged:
                _attack_type = AbstractAttack.CreateAttackType<GunnerAttack>(_unit_data, gameObject);
                break;

            default:
                throw new KeyNotFoundException("Incorrect attack type");
        }

        _attack_type.SetLayerSettingsObj(PlantSystem.UnitAdditionalSettings.CreateInstance(ColliderLayer));
    }

    private void FixedUpdate()
    {
        if (!_ready_to_actions)
        {
            StartCoroutine(Wait(_unit_data.start_timeout));
            _ready_to_actions = true;
        }

        if (_ready_to_actions && !_is_attacking) {
            StartCoroutine(AttackCycle());
        }
    }

    public IEnumerator AttackCycle()
    {
        _is_attacking = true;
        Vector2[] _directions_to_attack = CheckEnemy();

        if (_is_enemy_found) {
            int attack_num = 1;
            foreach (Vector2 _direction in _directions_to_attack)
            {
                Attack(_unit_data.damage, _direction);

                yield return new WaitForSecondsRealtime(_unit_data.GetSubAttackTimeout(attack_num));

                attack_num++;
            }

            yield return new WaitForSecondsRealtime(_unit_data.attack_timeout);
        }

        _is_enemy_found = false;
        _is_attacking = false;
    }

    public void Attack(int damage, Vector2 _direction)
    {
        _attack_type?.Attack(in _direction, damage);
    }

    public IEnumerator Wait(float duration)
    {
        yield return new WaitForSecondsRealtime(duration);
        _is_attacking = false;
    }

    public Vector2[] CheckEnemy()
    {
        return _attack_type?.CheckEnemy(ref _is_enemy_found) ?? new Vector2[1];
    }

    public void ResetUnit()
    {
        _is_attacking = false;
        _is_enemy_found = false;
    }
}
