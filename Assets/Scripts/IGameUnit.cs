using System.Collections;

using UnityEngine;

public interface IGameUnit
{
    void Attack(int damage, Vector2 _direction);
    Vector2[] CheckEnemy();
    Team getTeam();
    void Reset();
}