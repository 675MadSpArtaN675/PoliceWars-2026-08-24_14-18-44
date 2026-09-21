using UnityEngine;

public class GameSystemsInitializator : MonoBehaviour
{
    public bool IsDebugMode = false;
    public GameObject Ground;
    public GameObject SpawnerPrefab;

    public uint PrepareTime;
    public uint UnitSlotsCount;
    public float DistanceGroundAndEnemySpawners;

    public Team PlayerTeam;
    public TeamsRelationships FractionsRelationships;

    protected GameController _game_controller;
    protected SpawnController _spawn_system;
    protected DamageSystem _damage_system;
    protected DeathSystem _death_system;
    protected PlantSystem _plant_system;
    protected ProjectileSystem _projectile_system;

    private void Awake()
    {
        _game_controller = GetComponentInChildren<GameController>();
        _spawn_system = GetComponentInChildren<SpawnController>();
        _death_system = GetComponentInChildren<DeathSystem>();
        _damage_system = GetComponentInChildren<DamageSystem>();
        _plant_system = GetComponentInChildren<PlantSystem>();
        _projectile_system = GetComponentInChildren<ProjectileSystem>();
    }

    private void Start()
    {
        Init();
    }

    public virtual void Init()
    {
        ConfigureGameController();
        ConfigureDamageSystem();
        ConfigureDeathSystem();
        ConfigurePlantSystem();
        ConfigureSpawnSystem();
        ConfigureProjectileSystem();
    }

    protected virtual void ConfigureGameController()
    {
        if (_game_controller != null) {
            _game_controller.DebugMode = IsDebugMode;
            _game_controller.PlayerTeam = PlayerTeam;
            _game_controller.PrepareTime = PrepareTime;
        }
    }

    protected virtual void ConfigurePlantSystem()
    {
        if (_plant_system != null) {
            _plant_system.SlotsCount = UnitSlotsCount;
        }
    }

    protected virtual void ConfigureDamageSystem()
    {
        if (_damage_system != null) {
            _damage_system.Relationships = FractionsRelationships;
        }
    }

    protected virtual void ConfigureDeathSystem()
    { }

    protected virtual void ConfigureSpawnSystem()
    {
        if (_spawn_system != null) {
            _spawn_system.DistanceBetweenGroundAndSpawners = DistanceGroundAndEnemySpawners;
        }
    }

    protected virtual void ConfigureProjectileSystem()
    { }
}