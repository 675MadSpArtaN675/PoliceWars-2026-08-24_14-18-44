using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ProjectileSystem : MonoBehaviour, IProjectileSystem
{
    public static ProjectileSystem Instance { get; private set; }

    [Serializable]
    public struct ProjectileSubSystemInfo
    {
        [SerializeField]
        public string Name;

        [SerializeField]
        public GameObject SubSystemObj;
    }

    public List<ProjectileSubSystemInfo> subSystems = new();
    private Dictionary<string, IProjectileSystem> _sub_systems = new();
    private string choosen_projectile_type;

    private void Awake()
    {
        if (Instance == null) {
            Instance = this;
        }
    }

    private void Start()
    {
        foreach (ProjectileSubSystemInfo subSystem in subSystems)
        {
            if (subSystem.SubSystemObj?.GetComponent<Projectile>() != null)
            {
                string name = subSystem.Name?.Length > 0 ? subSystem.Name : subSystem.SubSystemObj.name;
                _sub_systems.Add(name, new ProjectileSubSystem(subSystem.SubSystemObj));
            }
        }
    }
    public void ChooseProjectile(string proj_type)
    {
        choosen_projectile_type = proj_type;
    }

    public void SpawnProjectile(BulletSettings settings)
    {
        if (settings.ProjectileName?.Length > 0)
        {
            ChooseProjectile(settings.ProjectileName);
        }

        _sub_systems.GetValueOrDefault(choosen_projectile_type, null)?.SpawnProjectile(settings);
    }

    public void DestroyProjectile(GameObject projectile_object)
    {
        _sub_systems.GetValueOrDefault(choosen_projectile_type, null)?.DestroyProjectile(projectile_object);
    }

    public void AddSubsystem(IProjectileSystem system){
        if (system is MonoBehaviour subsystem) {
            _sub_systems.Add(subsystem.name, system);
        }
    }

    public void RemoveSystem(string name)
    {
        if (_sub_systems.ContainsKey(name))
        {
            _sub_systems.Remove(name);
        }
    }

    public string[] GetSubSystemsByTitle()
    {
        return _sub_systems.Keys.ToArray();
    }
}