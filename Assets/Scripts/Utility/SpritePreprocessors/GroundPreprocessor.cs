using UnityEngine;

public class GroundPreprocessor : ISpritePreprocessor
{
    private ObjectInLinePlacer parent;

    public GroundPreprocessor(ObjectInLinePlacer parent)
    {
        this.parent = parent;
    }

    public void Process(ref SpriteRenderer _sprite)
    {
        Vector3 sprite_scale = _sprite.transform.localScale;
        Vector3 parent_scale = parent.GetScale();

        Debug.Log($"Name: {_sprite.name}. Old scale: {sprite_scale}. Parent x: {parent.GetWidth()}. Line x: {sprite_scale.x}");
        sprite_scale.x = parent_scale.x;

        Debug.Log($"Ground Preprop new size: {sprite_scale}");
        _sprite.transform.localScale = sprite_scale;
        _sprite.transform.localPosition = Vector3.zero;
        _sprite.sortingOrder = parent.GetComponentInChildren<SpriteRenderer>(true)?.sortingOrder ?? 0 + 2;
    }
}