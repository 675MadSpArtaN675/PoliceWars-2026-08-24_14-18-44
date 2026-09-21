using UnityEngine;

public struct BulletSettings
{
    public string ProjectileName;
    public Vector2 Direction;
    public Vector2 SpawnPosition;
    public Team OwnersTeam;

    public PlantSystem.UnitAdditionalSettings AdditionalSettings;

    public float Velocity;
    public int Damage;
}