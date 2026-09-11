using Unity.Android.Gradle.Manifest;
using UnityEngine;

public class MapBorderDeleter : MonoBehaviour
{
    [InspectorName("Check Interval")]
    public float check_interval = 2;

    private SpriteRenderer _sprite;
    private BoxCollider2D _collider;

    private void Start()
    {
        _collider = GetComponent<BoxCollider2D>();
        _sprite = GetComponentInChildren<SpriteRenderer>();

        _collider.size = _sprite.size;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log($"Border collide with {collision}");
        IGameUnit unit_component = collision.GetComponentInParent<IGameUnit>();

        if (unit_component is Projectile bullet)
        {
            ProjectileSystem.Instance.DestroyProjectile(bullet.gameObject);
        }
        else if (unit_component is MonoBehaviour unit_obj)
        {
            DeathSystem.Instance.DestroyUnit(unit_obj.gameObject);
        }
    }
}