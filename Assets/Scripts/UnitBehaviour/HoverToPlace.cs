using UnityEngine;

public class HoverToPlace : MonoBehaviour, IOnPlaceable
{
    private SpriteRenderer _sprite;

    [SerializeField]
    [InspectorName("Default sprite")]
    private Sprite _start_sprite;

    [SerializeField]
    [InspectorName("On entered sprite")]
    private Sprite _end_sprite;

    private bool _is_entered;

    private void Start()
    {
        _sprite = GetComponent<SpriteRenderer>();

        if (_sprite != null && _start_sprite == null) {
            _start_sprite = _sprite.sprite;
        }

        _is_entered = false;
    }

    private void OnMouseEnter()
    {
        _sprite.sprite = _end_sprite;
        _is_entered = true;
    }

    private void OnMouseExit()
    {
        _sprite.sprite = _start_sprite;
        _is_entered = false;
    }

    private void OnMouseDown()
    {
        if (_is_entered && PlantSystem.Instance != null) {
            Transform transform_of_this_object = GetComponent<Transform>();
            PlantSystem.Instance?.Plant(transform_of_this_object.position);

            Debug.Log("Planted...");
        }
    }
}
