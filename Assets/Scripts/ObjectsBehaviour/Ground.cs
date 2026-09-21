using System;
using System.Linq;
using UnityEngine;

public class Ground : ObjectInLinePlacer
{

    private int _index = 0;
    protected OnPlacerInitEvent OnLineInit;

    [InspectorName("Axis in inner object")]
    public Axis inner_axis = Axis.NoAxis;

    public override void ClearPlaces()
    {
        base.ClearPlaces();
        _index = 0;
    }
    public override Vector3[] GetLastElementPosition()
    {
        return _game_objects.SelectMany((GameObject _line) => _line.GetComponentInChildren<ObjectInLinePlacer>(true)?.GetLastElementPosition()).ToArray();
    }

    public void AddOnLineInitEvent(OnPlacerInitEvent _placer)
    {
        Debug.Log($"Adding line perfromer for placer: {name ?? "None"}");
        OnLineInit += _placer;
    }

    public void RemoveOnLineInitEvent(OnPlacerInitEvent _placer)
    {
        if (OnLineInit.GetInvocationList().Contains(_placer)) {
            Debug.Log($"Removing line perfromer for placer: {name ?? "None"}");
            OnLineInit += _placer;
        }
    }

    protected override void Init()
    {
        AddPreprocessor(new GroundPreprocessor(this));
    }

    protected override void PrereadyPlacesData(out float first_side_len, out Axis axis, out Vector3 direction)
    {
        first_side_len = 0;
        axis = Axis.NoAxis;
        direction = Vector3.zero;

        Debug.Log($"Place object: {PrefabToPlace?.name ?? "None"}. Component: {PrefabToPlace?.GetComponent<ObjectInLinePlacer>().name}");
        if (PrefabToPlace != null && PrefabToPlace.GetComponent<ObjectInLinePlacer>() is ObjectInLinePlacer) {
            base.PrereadyPlacesData(out first_side_len, out axis, out direction);

            if (inner_axis == Axis.NoAxis)
            {
                inner_axis = axis switch
                {
                    Axis.X => Axis.Y,
                    Axis.Y => Axis.X,

                    _ => Axis.NoAxis
                };
            }
            return;
        }

        throw new NullReferenceException($"Incorrect place prefab is unknown. Need to component that derived ObjectInLinePlacer");

    }

    protected override GameObject PlacePrefab(Vector3 position)
    {
        Debug.Log($"Placer: {gameObject?.name ?? "None"}");
        GameObject _result_game = null;
        if (PlacePrefabObj != null)
        {
            _result_game = Instantiate(PlacePrefabObj, position, Quaternion.Euler(Vector3.zero), transform);

            if (_result_game != null && _result_game.GetComponent<ObjectInLinePlacer>() is ObjectInLinePlacer placer)
            {
                placer.StartupInit = true;
                placer.IsPaintMarker = false;
                placer.axis_to_place = inner_axis;

                if (_index < SpawnController.sortingLayersForLines.Count){
                    placer.sortingLayerPlace = SpawnController.sortingLayersForLines[_index];
                }

                Debug.Log($"Placer {name ?? "None"} for line: {placer?.name ?? "None"} adding methods: {OnLineInit.GetInvocationList().Length}");
                if (OnLineInit.GetInvocationList().Length > 0)
                {
                    placer.AddOnInitEvent(OnLineInit);
                }

                _result_game.GetComponentInChildren<SpriteRenderer>().sortingOrder = _marker_sprite.sortingOrder + 2;
                _result_game.SetActive(true);

                _game_objects.Add(_result_game);
                _index++;
            }
        }

        return _result_game;
    }

}