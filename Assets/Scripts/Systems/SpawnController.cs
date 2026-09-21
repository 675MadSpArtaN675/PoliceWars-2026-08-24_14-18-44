using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Immutable;

using UnityEngine;

public class SpawnController : MonoBehaviour
{
    public static readonly ImmutableList<PlantSystem.SortingLayerLine> sortingLayersForLines = ImmutableList.CreateRange(new []{
        PlantSystem.SortingLayerLine.Line_1, PlantSystem.SortingLayerLine.Line_2,
        PlantSystem.SortingLayerLine.Line_3, PlantSystem.SortingLayerLine.Line_4,
        PlantSystem.SortingLayerLine.Line_5
    });

    public static SpawnController Instance { get; private set; }

    [InspectorName("Game ground")]
    public GameObject game_ground;

    [InspectorName("Enemy spawner prefab")]
    public GameObject game_spawner;

    public float DistanceBetweenGroundAndSpawners = 0;
    public bool IsSpawnerMarkersPaint = false;

    private List<GameObject> _spawners = new();

    private void Awake()
    {
        if (game_ground?.GetComponentInChildren<Ground>(true) == null)
        {
            throw new NullReferenceException("This object is not ground for policemans");
        }

        if (game_spawner?.GetComponentInChildren<EnemySpawner>(true) == null)
        {
            throw new NullReferenceException("Incorrect enemy spawner");
        }

        if (Instance == null)
        {
            Instance = this;
        }
    }

    private void OnEnable()
    {
        if (game_ground is GameObject && game_ground != null) {
            game_ground?.GetComponentInChildren<Ground>(true)?.AddOnLineInitEvent(OnLineInitEventPreformer);
        }
    }

    private void OnDisable()
    {
        if (game_ground is GameObject && game_ground != null) {
            game_ground?.GetComponentInChildren<Ground>(true)?.RemoveOnLineInitEvent(OnLineInitEventPreformer);
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
            spawner.IsMarkerPaint = IsSpawnerMarkersPaint;
            spawner.colliderLayer = (PlantSystem.CollideLineLayer) (int) layer;
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