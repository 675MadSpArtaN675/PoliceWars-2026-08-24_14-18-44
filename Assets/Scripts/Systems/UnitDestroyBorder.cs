using UnityEngine;

public class UnitDestroyBorder : MonoBehaviour
{
    private Collider2D _unit_colision;

    public void Awake()
    {
        _unit_colision = GetComponentInParent<Collider2D>();
    }

    public void FixedUpdate()
    {

    }
}
