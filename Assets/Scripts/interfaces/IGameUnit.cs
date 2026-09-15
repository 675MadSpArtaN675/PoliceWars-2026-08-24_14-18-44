using System.Collections;

using UnityEngine;

public interface IGameUnit : ITeamable
{
    public PlantSystem.CollideLineLayer ColliderLayer { get; set; }
    PlantSystem.UnitAdditionalSettings additionalSettings { get; set; }
    void SetOptionsByColliderLayer(PlantSystem.CollideLineLayer colliderLine);
    void SetOptions(PlantSystem.UnitAdditionalSettings colliderLine);

    void Attack(int damage, Vector2 _direction);
    IEnumerator Wait(float duration);
    Vector2[] CheckEnemy();

    void ResetUnit();
}