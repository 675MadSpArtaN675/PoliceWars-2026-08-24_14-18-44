using System;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class HoverToPlace : MonoBehaviour, IOnPlaceable
{
    private bool _is_entered;
    private Transform _transform;

    private SpriteRenderer _start_sprite;
    private SpriteRenderer _end_sprite;

    private SpriteRenderer[] _sprite;

    private BoxCollider2D _cell_collider;

    [SerializeField]
    [InspectorName("Default sprite")]
    private Sprite start_sprite;

    [SerializeField]
    [InspectorName("On entered sprite")]
    private Sprite end_sprite;
    public PlantSystem.SortingLayerLine layer = PlantSystem.SortingLayerLine.Bank;

    [InspectorName("Is blocked")]
    public bool IsBlocked;

    private void Start()
    {
        _sprite = GetComponentsInChildren<SpriteRenderer>(true);
        _transform = GetComponentInChildren<Transform>();

        if (_sprite != null && _sprite.Length > 0)
        {
            FillSprites(null, null);
        }
        else
        {
            FillSprites(start_sprite, end_sprite);
        }

        ConfigureCollider();

        _start_sprite.enabled = true;
        _end_sprite.enabled = false;

        _is_entered = false;
    }

    public void ConfigureCollider()
    {
        _cell_collider = GetComponent<BoxCollider2D>();
        _cell_collider.size = _start_sprite.size;
    }

    public void PlaceUnit()
    {
        PlantSystem.CollideLineLayer collide_layer = (PlantSystem.CollideLineLayer) layer;
        PlantSystem.SortingLayerLine sorting_layer = layer;

        PlantSystem.UnitAdditionalSettings ua_settings = PlantSystem.UnitAdditionalSettings.CreateInstance(collide_layer, sorting_layer);
        ua_settings.sorting_order += _end_sprite.sortingOrder + 3;

        PlantSystem.Instance?.PlaceUnit(_transform.position, ua_settings);
    }

    protected SpriteRenderer GetSpriteRendererByName(string name, string[] accessed_names)
    {
        return _sprite.First((SpriteRenderer render_comp) => render_comp.name.Contains(name) || accessed_names.Contains(render_comp.name, StringComparer.OrdinalIgnoreCase));
    }

    protected void FillSprites(Sprite sprite_1, Sprite sprite_2)
    {
        if (_start_sprite == null) {
            _start_sprite = CreateSprite(sprite_1, "(Basic)", new string[]{"Base"});
        }

        if (_end_sprite == null) {
            _end_sprite = CreateSprite(sprite_2, "(Selected)", new string[]{"Selected"});
        }
    }

    protected SpriteRenderer CreateSprite(Sprite value_sprite, string pattern_to_search, string[] args)
    {
        if (value_sprite == null)
        {
            return GetSpriteRendererByName(pattern_to_search, args);
        }
        else
        {
            SpriteRenderer new_renderer = gameObject.AddComponent<SpriteRenderer>();
            new_renderer.sprite = value_sprite;

            return new_renderer;
        }
    }

    private void OnMouseEnter()
    {
        if (PlantSystem.Instance.IsAnySlotChoosen() && !IsBlocked)
        {
            _end_sprite.enabled = true;
        }

        _is_entered = true;
    }

    private void OnMouseExit()
    {
        _end_sprite.enabled = false;
        _is_entered = false;
    }

    private void OnMouseDown()
    {
        if (!IsBlocked) {
            Debug.Log("Press mouse...");
            Vector2 ray_check_start_position = new Vector2(_transform.position.x, _transform.position.y);

            Collider2D _hit = SearchNearestFriendlyUnit(ray_check_start_position);

            if (_is_entered) {
                if (_hit == null) {
                    PlaceUnit();
                }
            }
        }
    }

    protected Collider2D SearchNearestFriendlyUnit(Vector2 ray_check_start_position)
    {
        Collider2D result_hit = null;
        Collider2D[] _collided_objects = SearchFriendlyUnits(ray_check_start_position);

        foreach (Collider2D _collider in _collided_objects)
        {
            Debug.Log($"Collided name: {_collider.name}");
        }

        Debug.Log(
            $"Collided slot: {_collided_objects.Length}. Is empty: {_collided_objects.Length < 1}. " +
            $"Is cursor entered: {_is_entered}. Can plant?: {_is_entered && _collided_objects.Length > 1}"
        );

        if (_is_entered && _collided_objects.Length > 0)
        {
            result_hit = _collided_objects.First();
            Debug.Log($"Slot collided unit: {(result_hit?.GetComponentInParent<IGameUnit>() as MonoBehaviour).gameObject.name ?? "None"}");
        }

        return result_hit;
    }
    protected Collider2D[] SearchFriendlyUnits(Vector2 ray_check_position)
    {
        return Physics2D.OverlapCircleAll(ray_check_position, _start_sprite.size.x * 0.6f)
            .Where((Collider2D _hit) => _hit.GetComponentInParent<ITeamable>() != null)
            .Where((Collider2D _hit) => _hit.GetComponentInParent<ITeamable>().Team == GameController.Instance.PlayerTeam)
            .OrderBy((Collider2D _hit) => Math.Abs(_hit.Distance(_cell_collider).distance))
            .ToArray();
    }
}
