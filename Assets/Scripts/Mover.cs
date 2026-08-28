using System;
using UnityEditor.Playables;
using UnityEngine;

public class Mover : MonoBehaviour
{
    public float Speed;

    public Vector2 Direction;

    private Transform _transform;
    private void Awake()
    {
        Speed = Speed / 100;

        Direction = UtilityGameFuncs.ClampVector(Direction, -1, 1);

        _transform = GetComponent<Transform>();
    }

    private void FixedUpdate()
    {
        Vector2 speed = Direction * Speed;

        Vector3 pos = _transform.position;
        pos.x += speed.x;
        pos.y += speed.y;

        _transform.position = pos;
    }
}