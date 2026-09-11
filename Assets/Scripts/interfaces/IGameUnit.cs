using System.Collections;

using UnityEngine;

public interface IGameUnit : ITeamable
{
    void Attack(int damage, Vector2 _direction);
    IEnumerator Wait(float duration);
    Vector2[] CheckEnemy();
    void Reset();
}