using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public abstract class AbstractAttack
{
    public static T CreateAttackType<T>(UnitSettings settings, GameObject obj) where T: AbstractAttack, new()
    {
        T instance = new T();
        instance.InitializeAttack(settings, obj);

        return instance;
    }

    public static EmptyAttack CreateEmpty(UnitSettings settings, GameObject obj)
    {
        return CreateAttackType<EmptyAttack>(settings, obj);
    }

    private SortedList<float, GameObject> _cached_entities;

    protected virtual bool isEnableCaching => false;
    protected GameObject _unit_game_obj;
    protected Transform _unit_transformation;
    protected Mover _mover;
    protected Team _unit_team;
    protected PlantSystem.UnitAdditionalSettings _additional_settings;
    protected UnitSettings _unit_data;

    public AbstractAttack(UnitSettings settings, GameObject obj)
    {
        InitializeAttack(settings, obj);
    }

    public virtual void InitializeAttack(UnitSettings settings, GameObject obj)
    {
        _unit_game_obj = obj;
        _unit_data = settings;
        _unit_team = obj?.GetComponentInParent<ITeamable>()?.Team ?? Team.NoTeam;
        _unit_transformation = obj?.GetComponentInParent<Transform>();
        _mover = obj?.GetComponentInParent<Mover>();

        if (isEnableCaching)
        {
            EnableFoundEntityCache();
        }
        else
        {
            DisableFoundEntityCache();
        }
    }

    public abstract void Attack(in Vector2 _direction, int damage);
    public abstract Vector2[] CheckEnemy(ref bool _is_enemy_found);

    public void EnableFoundEntityCache()
    {
        if (_cached_entities == null) {
            _cached_entities = new SortedList<float, GameObject>();
        }
    }

    public void DisableFoundEntityCache()
    {
        _cached_entities = null;
    }

    public void SetLayerSettingsObj(PlantSystem.UnitAdditionalSettings _uas)
    {
        _additional_settings = _uas;
    }

    public PlantSystem.UnitAdditionalSettings GetLayerSettingsObj()
    {
        return _additional_settings;
    }

    protected void AddEntityToCache(GameObject value)
    {
        if (_cached_entities != null) {
            Vector3 enemy_pos = value.GetComponent<Transform>().position;
            Vector3 _unit_pos = _unit_transformation.position;

            float distance = Mathf.Abs(Vector3.Distance(_unit_pos, enemy_pos));
            Debug.Log($"Caching: {value.name}. Distance to him: {distance}");

            _cached_entities.Add(distance, value);
        }
    }

    protected void AddEntities(GameObject[] value)
    {
        if (_cached_entities != null) {
            Debug.Log("Adding some entities...");
            foreach (GameObject _choosed_obj in value)
            {
                Debug.Log($"Found entity {_choosed_obj.name}");
                AddEntityToCache(_choosed_obj);
            }
        }
    }

    protected bool isCacheEmpty()
    {
        return _cached_entities == null || _cached_entities.Count < 1;
    }

    protected GameObject GetFromCache()
    {
        if (!isCacheEmpty()){
            KeyValuePair<float, GameObject>? cached_value = _cached_entities?.First();

            if (cached_value != null)
            {
                float distance = cached_value.Value.Key;
                GameObject obj = cached_value.Value.Value;

                _cached_entities.Remove(distance);

                return obj;
            }
        }

        return null;
    }

    protected void ClearCache()
    {
        if (!isCacheEmpty())
        {
            _cached_entities?.Clear();
        }
    }
}