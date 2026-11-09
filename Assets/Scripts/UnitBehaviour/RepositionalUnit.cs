using System.Collections;
using UnityEngine;

public class RepositionableUnit : MonoBehaviour
{
    private Vector3 _next_point;
    private Vector3 _end_point;

    private Transform _transform;
    private Mover _mover;

    private void Start()
    {
        _transform = transform;
        _mover = GetComponent<Mover>();

        Clear();
    }

    public virtual void Move(Vector2 distance_to_move)
    {
        Vector3 _distance_to_move_in_3d = distance_to_move;
        Vector3 _end_position = _transform.position + _distance_to_move_in_3d;

        MoveTo(_end_position);
    }

    public virtual void Move(int on_horizontal_cell, int on_vertical_cell)
    {
    }

    public virtual void MoveTo(Vector2 position)
    {
        _next_point = new Vector3(_transform.position.x, position.y);
        _end_point = position;

        Debug.Log($"TP: {transform.position}. NP: {_next_point}. EP: {_end_point}");
        _mover.MoveTo(_next_point);
        _mover.MoveTo(_end_point);
    }

    public virtual void MoveTo(int horizontal_cell, int vertical_cell)
    {
    }

    public void Clear()
    {
        _next_point = Vector3.zero;
        _end_point = Vector3.zero;

        _mover.ResetMove();
    }
}
