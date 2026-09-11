using UnityEngine;

public class MapPlayerDefeat : MonoBehaviour
{
    [InspectorName("Player team")]
    public Team team;

    private SpriteRenderer _sprite;
    private BoxCollider2D _collider;

    private void Start()
    {
        _sprite = GetComponentInParent<SpriteRenderer>();
        _collider = GetComponentInParent<BoxCollider2D>();

        _collider.size = _sprite.size;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        ITeamable unit = collision.GetComponentInParent<ITeamable>();

        if (unit is BattleBrain ready_unit)
        {
            Debug.Log($"Bank collision with unit: {collision.gameObject.name}");
            if (DamageSystem.Instance.Relationships.GetEnemyTeams(team).Contains(ready_unit.Team))
            {
                GameController.Instance?.LoseGame();
                GameController.Instance?.PauseGame();
            }
        }
    }
}