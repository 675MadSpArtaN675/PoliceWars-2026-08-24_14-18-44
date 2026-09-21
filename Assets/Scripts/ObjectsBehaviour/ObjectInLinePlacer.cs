using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ObjectInLinePlacer : MonoBehaviour
{
    public delegate void OnPlacerInitEvent(ObjectInLinePlacer _placer);

    public enum Axis
    {
        NoAxis,
        X,
        Y
    }

    [InspectorName("Interval between cells")]
    public float Dx = 0;
    public bool IsEnabled = true;
    public bool StartupInit = true;
    public bool IsPaintMarker = true;
    public GameObject PrefabToPlace = null;
    public Axis axis_to_place = Axis.NoAxis;
    public PlantSystem.SortingLayerLine sortingLayerPlace = PlantSystem.SortingLayerLine.Bank;

    [SerializeField]
    protected OnPlacerInitEvent OnPlacerInit;

    protected GameObject PlacePrefabObj = null;
    protected SpriteRenderer _marker_sprite;
    protected List<ISpritePreprocessor> _preprocessors = new();
    protected List<GameObject> _game_objects;

    private void Awake()
    {
        IsPaintMarker = GameController.Instance?.DebugMode ?? IsPaintMarker;

        _game_objects = new List<GameObject>();
        _marker_sprite = GetComponentInChildren<SpriteRenderer>();
    }

    private void Start()
    {
        Debug.Log($"Placer {gameObject.name}. It size: {CalculateSizeOfMarker(Axis.X)}x{CalculateSizeOfMarker(Axis.Y)}");
        if (!IsPaintMarker)
        {
            _marker_sprite.enabled = false;
        }

        Init();

        if (StartupInit) {
            UpdatePlaces();
        }
    }

    protected virtual void Init()
    { }

    public float GetWidth()
    {
        return getAxisSize(_marker_sprite, Axis.X);
    }

    public float GetHeight()
    {
        return getAxisSize(_marker_sprite, Axis.Y);
    }

    public void SetWidth(float value)
    {
        setAxisSize(_marker_sprite, Axis.X, value);
    }

    public void SetHeight(float value)
    {
        setAxisSize(_marker_sprite, Axis.Y, value);
    }

    public void SetScale(Vector3 scale_value)
    {
        _marker_sprite.transform.localScale = scale_value;
    }

    public Vector3 GetScale()
    {
        return _marker_sprite.transform.localScale;
    }

    public Vector2 GetPlacableSize()
    {
        return new Vector2(CalculateSizeOfPlace(Axis.X), CalculateSizeOfPlace(Axis.Y));
    }

    public int GetObjectsCount()
    {
        return _game_objects.Count;
    }

    public void AddOnInitEvent(OnPlacerInitEvent _event)
    {
        Debug.Log($"Adding perfromer for placer: {name ?? "None"}");
        OnPlacerInit += _event;
    }

    public void RemoveOnInitEvent(OnPlacerInitEvent _event)
    {
        if (OnPlacerInit.GetInvocationList().Contains(_event))
        {
            Debug.Log($"Removing perfromer for placer: {name ?? "None"}");
            OnPlacerInit -= _event;
        }
    }

    public void UpdatePlaces()
    {
        if (IsEnabled) {
            Vector3 direction;
            float first_side_len;
            Axis axis;

            ClearPlaces();
            PrereadyPlacesData(out first_side_len, out axis, out direction);
            PlaceAll(first_side_len, axis, direction);

            OnPlacerInit?.Invoke(this);
        }
    }

    public virtual void ClearPlaces()
    {
        foreach (GameObject _game_object in _game_objects)
        {
            _game_object.SetActive(false);
            Destroy(_game_object);
        }

        _game_objects.Clear();
    }

    public void AddPreprocessor(ISpritePreprocessor _sprite_preprocessor)
    {
        if (_sprite_preprocessor != null) {
            _preprocessors.Add(_sprite_preprocessor);
        }
    }

    public void RemovePreprocessor(ISpritePreprocessor _sprite_preprocessor)
    {
        if (_sprite_preprocessor != null) {
            _preprocessors.Remove(_sprite_preprocessor);
        }
    }

    public void RemovePreprocessorByIndex(int index)
    {
        if (index > 0 && index < _preprocessors.Count)
        {
            _preprocessors.RemoveAt(index);
        }
    }

    public ISpritePreprocessor GetPreprocessor(int index)
    {
        if (index > 0 && index < _preprocessors.Count)
        {
            return _preprocessors[index];
        }

        return null;
    }

    public virtual Vector3[] GetLastElementPosition()
    {
        Debug.Log($"Placers count {_game_objects.Count} in object {name ?? "None"}");
        if (_game_objects.Count > 0) {
            return new Vector3[] { _game_objects.OrderBy((GameObject com) => com.transform.position.x).Last().transform.position };
        }

        return new Vector3[0];
    }

    protected virtual void PrereadyPlacesData(out float first_side_len, out Axis axis, out Vector3 direction)
    {
        Debug.Log($"Choosen placer prefab: {PrefabToPlace?.name ?? "None"}");
        if (PrefabToPlace != null) {
            SpriteRenderer sprite;

            if (axis_to_place == Axis.NoAxis) {
                if (_marker_sprite.size.x >= _marker_sprite.size.y)
                {
                    axis = Axis.X;
                    direction = ChooseAxisDirection(axis);
                }
                else
                {
                    axis = Axis.Y;
                    direction = ChooseAxisDirection(axis);
                }
            }
            else
            {
                axis = chooseAxis(axis_to_place);
                direction = ChooseAxisDirection(axis);
            }

            sprite = GetPrefabSprite(axis);
            first_side_len = getAxisSize(in sprite, axis) + Dx;

            return;
        }

        throw new NullReferenceException("Place prefab is unknown");
    }


    protected void PlaceAll(float first_side_len, Axis axis, Vector3 direction) {
        Vector3 placer_position = GetRealSpawnPoint(axis, first_side_len);
        int count_of_places = (int) Math.Floor(CalculateSizeOfMarker(axis) / first_side_len);
        Debug.Log(
            $"Placer: {gameObject?.name ?? "None"}." +
            $"\n\tCount of places = {count_of_places}." +
            $"\n\tLen: {CalculateSizeOfMarker(axis)}." +
            $"\n\tFirst Side len: {first_side_len}." +
            $"\n\tAxis: {axis}." +
            $"\n\tDirection {direction}"
        );

        Debug.Log($"Start place position: {placer_position}");
        for (int i = 0; i < count_of_places; i++)
        {
            Vector3 result_position = placer_position + direction * first_side_len * i;
            GameObject new_placer = PlacePrefab(result_position);

            Debug.Log($"Placer {name ?? "None"}. New placer: {new_placer?.name ?? "None"} Prev place position: {placer_position}. Position: {result_position}. Objects count: {_game_objects.Count}");
        }
    }

    protected virtual GameObject PlacePrefab(Vector3 position)
    {
        Debug.Log($"Placer class: {gameObject?.name ?? "None"}");
        GameObject new_place = Instantiate(PlacePrefabObj, position, Quaternion.Euler(Vector3.zero), transform);
        if (new_place != null) {
            Debug.Log($"Prefab name: {new_place?.name ?? "None"}");
            HoverToPlace place_component = new_place.GetComponent<HoverToPlace>();

            if (place_component == null)
            {
                throw new NullReferenceException("No place component on object");
            }

            place_component.sortingLayerForUnit = sortingLayerPlace;
            new_place.GetComponentInChildren<SpriteRenderer>().sortingOrder = _marker_sprite.sortingOrder + 2;

            new_place.SetActive(true);
        }

        _game_objects.Add(new_place);
        return new_place;
    }

    protected Vector3 GetRealSpawnPoint(Axis spawn_axis, float side_len)
    {
        if (_marker_sprite != null) {
            Vector3 position = _marker_sprite.transform.position;
            Vector3 transform_pos;

            if (spawn_axis == Axis.X)
            {
                transform_pos = new Vector3(CalculateSizeOfMarker(Axis.X) / 2 - side_len / 2, 0, 0);
            }
            else
            {
                transform_pos = new Vector3(0, CalculateSizeOfMarker(Axis.Y) / 2 - side_len / 2, 0);
            }


            return position + transform_pos;
        }

        return Vector3.zero;
    }

    protected float CalculateSizeOfSprite(in SpriteRenderer place_sprites, Axis axis)
    {
        return getAxisSize(in place_sprites, axis);
    }

    protected float CalculateSizeOfMarker(Axis axis)
    {
        if (_marker_sprite != null) {
            float field = getAxisSize(in _marker_sprite, axis);

            return field;
        }

        return 0f;
    }

    protected float CalculateSizeOfPlace(Axis axis)
    {
        SpriteRenderer place_sprite = GetPrefabSprite(axis);

        return getAxisSize(in place_sprite, axis);
    }

    protected SpriteRenderer GetPrefabSprite(Axis axis)
    {
        SpriteRenderer _first_sprite = MaxSizedSprite(axis);
        _first_sprite.sortingOrder = _marker_sprite?.sortingOrder ?? 0 + 2;
        ObjectSpritesPreprocessorStart(ref _first_sprite);

        return _first_sprite;
    }

    protected SpriteRenderer MaxSizedSprite(Axis axis)
    {
        if (PlacePrefabObj == null) {
            PlacePrefabObj = Instantiate(PrefabToPlace);
            PlacePrefabObj.SetActive(false);
        }

        SpriteRenderer[] _spir = PlacePrefabObj
                ?.GetComponentsInChildren<SpriteRenderer>()
                ?.OrderByDescending((SpriteRenderer sprite) => getAxisSize(in sprite, axis)).ToArray();

        foreach (SpriteRenderer _s in _spir)
        {
            Debug.Log($"Place sprite: {_s.name}. Size: {_s.bounds.size.x}x{_s.bounds.size.y}");
        }

        return _spir.First();
    }

    protected void ObjectSpritesPreprocessorStart(ref SpriteRenderer sprite)
    {
        if (_preprocessors.Count > 0)
        {
            Debug.Log($"Placer preprocessors started. It count: {_preprocessors.Count}");
            foreach (ISpritePreprocessor preprocessor in _preprocessors)
            {
                preprocessor.Process(ref sprite);
            }
        }
    }

    protected Axis chooseAxis(Axis calculated_axis)
    {
        return axis_to_place == Axis.NoAxis ? calculated_axis : axis_to_place;
    }

    protected void setAxisSize(in SpriteRenderer sprite, Axis axis, float value) {
        if (axis == Axis.X)
        {
            Vector3 now_transform = sprite.transform.localScale;
            now_transform.x = value;

            sprite.transform.localScale = now_transform;
        }
        else
        {
            Vector3 now_transform = sprite.transform.localScale;
            now_transform.y = value;

            sprite.transform.localScale = now_transform;
        }
    }

    protected Axis GetPerpendicularAxis(Axis input_axis)
    {
        return input_axis switch
        {
            Axis.X => Axis.Y,
            Axis.Y => Axis.X,

            _ => throw new NullReferenceException("Incorrect value")
        };
    }

    protected Vector3 ChooseAxisDirection(Axis axis)
    {
        return axis switch
        {
            Axis.X => new Vector3(-1, 0, 0),
            Axis.Y => new Vector3(0, -1, 0),

            Axis.NoAxis => Vector3.zero
        };
    }

    protected float getAxisSize(in SpriteRenderer sprite, Axis axis) {
        return axis switch {
            Axis.X => sprite.bounds.size.x,
            Axis.Y => sprite.bounds.size.y,

            Axis.NoAxis => 0f
        };
    }
}
