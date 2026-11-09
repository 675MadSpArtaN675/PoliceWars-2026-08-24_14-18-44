using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public PlantSystem.CollideLineLayer colliderLayer = PlantSystem.CollideLineLayer.BankLayer;
    public Vector2 direction = new Vector2(-1, 0);
    public Vector2 posInterval = new Vector2(0, 0);
    public bool IsMarkerPaint = false;

    private System.Random _random;
    private Transform _transform;

    private void Start()
    {
        _random = new();
        _transform = GetComponent<Transform>();

        loadMarkerStatus();
    }

    private void Update()
    {
        loadMarkerStatus();
    }


    protected void loadMarkerStatus()
    {
        IsMarkerPaint = GameController.Instance.DebugMode;

        if (!IsMarkerPaint) {
            SpriteRenderer[] _sprites = GetComponentsInChildren<SpriteRenderer>();

            foreach (SpriteRenderer _sprite in _sprites)
            {
                if (_sprite.name == "Body")
                {
                    _sprite.enabled = false;
                }
            }
        }
    }

    public GameObject Spawn(GameObject _obj)
    {
        if (_obj.GetComponent<IGameUnit>() is IGameUnit) {
            Debug.Log($"Spawn interval: {calculateInterval()}");
            GameObject new_obj = Instantiate(_obj, _transform.position + calculateInterval(), Quaternion.Euler(0, 0, 0));
            new_obj.SetActive(true);

            if (new_obj.GetComponentInChildren<Mover>() is Mover _mover && new_obj.GetComponentInChildren<BattleBrain>() is BattleBrain _brain)
            {
                _brain.ColliderLayer = colliderLayer;
                _mover.Direction = direction;

                _brain.Reconfigure();
            }

            return new_obj;
        }

        return null;
    }

    protected Vector3 calculateInterval()
    {
        float start_x = 0, start_y = 0;
        float end_x = posInterval.x, end_y = posInterval.y;

        if (start_x == end_x && start_y == end_y)
        {
            return Vector3.zero;
        }

        if (start_x > end_x)
        {
            float temp = start_x;
            start_x = end_x;
            end_x = temp;
        }

        if (start_y > end_y)
        {
            float temp = start_y;
            start_y = end_y;
            end_y = temp;
        }

        return new Vector3(GenerateRandomfloat(start_x, end_x), GenerateRandomfloat(start_y, end_y), 0);
    }

    protected float GenerateRandomfloat(float start_, float end_)
    {
        float divider = 1000;
        float _interval = (float) _random.Next((int) (Mathf.Floor(start_) * divider), (int) (Mathf.Ceil(end_) * divider)) / divider;

        Debug.Log($"Spawner. Interval: {_interval}");
        return _interval;
    }

    public List<GameObject> SpawnAny(GameObject _obj, int count)
    {
        List<GameObject> _result = new();
        for (uint i = 0; i < count; i++)
        {
            GameObject unit = Spawn(_obj);

            if (unit != null) {
                _result.Add(unit);
            }
        }

        return _result;
    }

    public List<GameObject> SpawnRandomCount(GameObject _obj, int min_count, int max_count)
    {
        List<GameObject> _result = new();
        if (min_count > 0) {
            System.Random random = new System.Random();
            int choosen_count = random.Next(min_count, max_count);

            _result.AddRange(SpawnAny(_obj, choosen_count));
        }

        return _result;
    }

    protected Vector2 RandomCoordinateInZone(Vector2 up_vec_1, Vector2 left_vec_2)
    {
        System.Random _rand = new System.Random();
        double x = _rand.NextDouble() * (left_vec_2.x - up_vec_1.x) + up_vec_1.x;
        double y = _rand.NextDouble() * (left_vec_2.y - up_vec_1.y) + up_vec_1.y;

        return new Vector2((float) x, (float) y);
    }
}
