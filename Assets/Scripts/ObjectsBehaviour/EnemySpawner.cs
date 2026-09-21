using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public PlantSystem.CollideLineLayer colliderLayer = PlantSystem.CollideLineLayer.BankLayer;
    public Vector2 direction = new Vector2(-1, 0);
    public bool IsMarkerPaint = false;

    private Transform _transform;

    private void Start()
    {
        IsMarkerPaint = GameController.Instance.DebugMode;
        _transform = GetComponent<Transform>();

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

    public void Spawn(GameObject _obj)
    {
        if (_obj.GetComponent<IGameUnit>() is IGameUnit) {
            GameObject new_obj = Instantiate(_obj, _transform.position, Quaternion.Euler(0, 0, 0));
            new_obj.SetActive(true);

            if (_obj.GetComponent<Mover>() is Mover _mover && _obj.GetComponent<BattleBrain>() is BattleBrain brain)
            {
                brain.ColliderLayer = colliderLayer;
                _mover.Speed = 0;
                _mover.Direction = direction;
            }
        }
    }

    public void SpawnAny(GameObject _obj, int count)
    {
        for (uint i = 0; i < count; i++)
        {
            Spawn(_obj);
        }
    }

    public void SpawnRandomCount(GameObject _obj, int min_count, int max_count)
    {
        if (min_count > 0) {
            System.Random random = new System.Random();
            int choosen_count = random.Next(min_count, max_count);

            SpawnAny(_obj, choosen_count);
        }
    }

    protected Vector2 RandomCoordinateInZone(Vector2 up_vec_1, Vector2 left_vec_2)
    {
        System.Random _rand = new System.Random();
        double x = _rand.NextDouble() * (left_vec_2.x - up_vec_1.x) + up_vec_1.x;
        double y = _rand.NextDouble() * (left_vec_2.y - up_vec_1.y) + up_vec_1.y;

        return new Vector2((float) x, (float) y);
    }
}
