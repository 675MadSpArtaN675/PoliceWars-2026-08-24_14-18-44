using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;

using UnityEngine;
using System.Collections.Specialized;

public class SpawnController : MonoBehaviour
{
    public delegate void onEventDelegate(List<GameObject> _objects);

    public static readonly ImmutableList<PlantSystem.SortingLayerLine> sortingLayersForLines = ImmutableList.CreateRange(new []{
        PlantSystem.SortingLayerLine.Line_1, PlantSystem.SortingLayerLine.Line_2,
        PlantSystem.SortingLayerLine.Line_3, PlantSystem.SortingLayerLine.Line_4,
        PlantSystem.SortingLayerLine.Line_5
    });
    public static readonly ImmutableList<PlantSystem.CollideLineLayer> colliderLayersForLines = ImmutableList.CreateRange(new []{
        PlantSystem.CollideLineLayer.AttackLine_1, PlantSystem.CollideLineLayer.AttackLine_2,
        PlantSystem.CollideLineLayer.AttackLine_3, PlantSystem.CollideLineLayer.AttackLine_4,
        PlantSystem.CollideLineLayer.AttackLine_5
    });

    public static SpawnController Instance { get; private set; }

    [InspectorName("Game ground")]
    public GameObject game_ground;

    [InspectorName("Enemy spawner prefab")]
    public GameObject game_spawner;
    public Vector3 posInterval = Vector3.zero;
    public Action OnCollectionEmpty;

    public float DistanceBetweenGroundAndSpawners = 0;

    private onEventDelegate _on_spawn_check;

    protected ObservableCollection<GameObject> _spawned_enemies;
    private List<GameObject> _spawners;

    private void Awake()
    {
        _spawned_enemies = new();
        _spawners = new();

        if ((game_ground != null && game_ground?.GetComponentInChildren<Ground>(true) == null) || game_ground == null)
        {
            throw new NullReferenceException("This object is not ground for policemans");
        }

        if ((game_spawner != null && game_spawner?.GetComponentInChildren<EnemySpawner>(true) == null) || game_spawner == null)
        {
            throw new NullReferenceException("Incorrect enemy spawner");
        }

        if (Instance == null)
        {
            Instance = this;
        }

        _spawned_enemies.CollectionChanged += IsSpawnedEnemiesEmpty;
    }

    protected void IsSpawnedEnemiesEmpty(object sender, NotifyCollectionChangedEventArgs args)
    {
        if (sender is ObservableCollection<GameObject> _sender_collection)
        {
            if (_sender_collection.Count <= 0)
            {
                OnCollectionEmpty?.Invoke();
            }
        }
    }

    private void OnEnable()
    {
        if (game_ground != null && game_ground is GameObject) {
            game_ground?.GetComponentInChildren<Ground>(true)?.AddOnLineInitEvent(OnLineInitEventPreformer);
            SetPosIntervalOfSpawners(posInterval);
        }
    }

    private void OnDisable()
    {
        if (game_ground != null && game_ground is GameObject) {
            game_ground?.GetComponentInChildren<Ground>(true)?.RemoveOnLineInitEvent(OnLineInitEventPreformer);
            SetPosIntervalOfSpawners(Vector3.zero);
        }
    }

    private void SetPosIntervalOfSpawners(Vector3 pos_interval)
    {
        foreach (GameObject _spawner in _spawners)
        {
            EnemySpawner _spawner_component = _spawner.GetComponentInChildren<EnemySpawner>();

            if (_spawner_component != null)
            {
                _spawner_component.posInterval = pos_interval;
            }
        }
    }

    protected void OnLineInitEventPreformer(ObjectInLinePlacer ground)
    {
        Vector3[] _last_positions = ground.GetLastElementPosition();
        Debug.Log($"Line: {ground?.name ?? "None"}. Positions count: {_last_positions.Length}");

        if (_last_positions.Length > 0)
        {
            Vector3 _position = _last_positions.First();
            _position.x += ground.GetPlacableSize().x;

            CreateSpawner(_position, ground.sortingLayerPlace);
        }
    }

    public void LinkOnSpawnEvent(onEventDelegate event_listener)
    {
        _on_spawn_check += event_listener;
    }

    public bool IsElementSpawnedByController(GameObject _obj)
    {
        return _spawned_enemies.Contains(_obj);
    }

    public void DestroyElement(GameObject _obj)
    {
        if (IsElementSpawnedByController(_obj)) {
            _spawned_enemies.Remove(_obj);
        }
    }

    public List<PlantSystem.CollideLineLayer> GetCollideLayers()
    {
        return _spawned_enemies.Select((GameObject _obj) => _obj.GetComponentInChildren<BattleBrain>(true)?.ColliderLayer ?? PlantSystem.CollideLineLayer.BankLayer).Distinct().ToList();
    }

    public bool IsHasAliveEnemies()
    {
        return _spawned_enemies.Count > 0;
    }

    public List<GameObject> SpawnEnemy(GameObject enemy_unit, int spawner_num, int min_count, int max_count = -1)
    {
        List<GameObject> _spawned = new();
        if (spawner_num >= 0 && spawner_num < _spawners.Count){
            GameObject _spawner_obj = _spawners[spawner_num];

            if (_spawner_obj.GetComponentInChildren<EnemySpawner>() is EnemySpawner spawner) {
                if (max_count < 1) {
                    if (min_count == 1)
                    {
                        _spawned.Add(spawner.Spawn(enemy_unit));
                    }
                    else if (min_count > 1)
                    {
                        _spawned.AddRange(spawner.SpawnAny(enemy_unit, min_count));
                    }
                }
                else
                {
                    if (max_count < min_count)
                    {
                        int temp = min_count;

                        min_count = max_count;
                        max_count = temp;
                    }

                    _spawned.AddRange(spawner.SpawnRandomCount(enemy_unit, min_count, max_count));
                }
            }
        }

        foreach (GameObject _obj in _spawned) {
            _spawned_enemies.Add(_obj);
        }
        _on_spawn_check?.Invoke(_spawned);

        return _spawned;
    }

    public void CreateSpawner(Vector3 position, PlantSystem.SortingLayerLine sorting_layer)
    {
        if (sorting_layer > PlantSystem.SortingLayerLine.Bank)
        {
            CreateSpawner(position, -1, sorting_layer);
        }
    }

    public void CreateSpawner(Vector3 position, int index_of_line)
    {
        if (index_of_line >= 0)
        {
            CreateSpawner(position, index_of_line);
        }
    }

    protected void CreateSpawner(Vector3 place_position, int index_of_line, PlantSystem.SortingLayerLine? sorting_layer = null)
    {
        Vector3 position_dx = new Vector3(1, 0, 0) * DistanceBetweenGroundAndSpawners;

        GameObject enemy_spawner = Instantiate(game_spawner, place_position + position_dx, Quaternion.Euler(Vector3.zero), game_ground.transform);

        PlantSystem.SortingLayerLine? layer = null;
        if (index_of_line >= 0 && index_of_line < sortingLayersForLines.Count && !sorting_layer.HasValue)
        {
            layer = sortingLayersForLines[index_of_line];
        }
        else if (sorting_layer.HasValue)
        {
            layer = sorting_layer;
        }
        else
        {
            layer = PlantSystem.SortingLayerLine.Line_1;
        }

        Debug.Log($"Created enemy spawner '{enemy_spawner?.name ?? "None"}' on position: {place_position} and on layer: {layer?.ToString() ?? "None"}");

        if (layer.HasValue && enemy_spawner.GetComponentInChildren<EnemySpawner>(true) is EnemySpawner spawner)
        {
            spawner.colliderLayer = (PlantSystem.CollideLineLayer) (int) layer;
            spawner.posInterval = posInterval;
        }

        enemy_spawner.SetActive(true);

        if(enemy_spawner != null) {
            _spawners.Add(enemy_spawner);
        }
    }

    public void CreateAllSpawners(Ground _ground_component)
    {
        Debug.Log("Creating enemy spawners");

        Vector3[] last_places_on_lines = _ground_component.GetLastElementPosition();
        int places_len = last_places_on_lines.Count();
        Debug.Log($"Places count: {places_len}");

        if (last_places_on_lines.Length > 0)
        {
            int index_of_line = 0;

            while (index_of_line < places_len)
            {
                Vector3 place_position = last_places_on_lines[index_of_line] + new Vector3(0, 1, 0);
                CreateSpawner(place_position, index_of_line);

                index_of_line++;
            }
        }
    }
}