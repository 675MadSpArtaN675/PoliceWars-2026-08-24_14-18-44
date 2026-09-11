using System.Collections.Generic;

using UnityEngine;

public class ProjectileSystem : MonoBehaviour
{
    public static ProjectileSystem Instance { get; private set; }

    [InspectorName("Bullet")]
    public GameObject bullet_prefab;

    private Queue<GameObject> _bullets_cache = new Queue<GameObject>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    public void SpawnProjectile(BulletSettings settings)
    {
        Debug.Log($"Count queue: {_bullets_cache.Count}. Input bullet team: {settings.OwnersTeam}");
        if (_bullets_cache.Count > 0)
        {
            GameObject _exited_object = _bullets_cache.Dequeue();
            ProjectileConfigure(_exited_object, in settings);
            Debug.Log($"Getting from cache. Count queue: {_bullets_cache.Count}");

            _exited_object.SetActive(true);
        }
        else
        {
            ProjectileConfigure(bullet_prefab, in settings);

            Instantiate(bullet_prefab, bullet_prefab.transform.position, Quaternion.Euler(Vector3.zero));
        }
    }

    public void DestroyProjectile(GameObject projectile_object)
    {
        Debug.Log($"Destroying projectile: {projectile_object}");
        Projectile _projectile = projectile_object.GetComponentInChildren<Projectile>();
        Mover _mover = projectile_object.GetComponentInChildren<Mover>();

        if (_projectile != null) {
            if (_mover != null)
            {
                _mover.MoveStop();
            }

            projectile_object.SetActive(false);

            _bullets_cache.Enqueue(projectile_object);
        }
    }

    protected void ProjectileConfigure(GameObject projectile_object, in BulletSettings settings)
    {
        Debug.Log($"Configuring bullet for team {settings.OwnersTeam}: {settings.Damage}");

        Transform transform = projectile_object.GetComponent<Transform>();
        Mover mover = projectile_object.GetComponent<Mover>();
        Projectile _projectile_component = projectile_object.GetComponent<Projectile>();

        transform.position = new Vector3(settings.SpawnPosition.x, settings.SpawnPosition.y, 0);

        mover.Direction = settings.Direction;
        mover.Speed = settings.Velocity;

        _projectile_component.Damage = settings.Damage;
        _projectile_component.Team = settings.OwnersTeam;

        mover.Reset();
        _projectile_component.Reset();
    }
}
