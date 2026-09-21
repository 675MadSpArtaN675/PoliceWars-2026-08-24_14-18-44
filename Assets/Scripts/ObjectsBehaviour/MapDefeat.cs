using UnityEngine;

public class MapPlayerDefeat : MonoBehaviour
{
    public bool IsMarkerPaint = false;

    private SpriteRenderer _sprite;
    private BoxCollider2D _collider;

    private void Start()
    {
        IsMarkerPaint = GameController.Instance?.DebugMode ?? IsMarkerPaint;

        _sprite = GetComponentInChildren<SpriteRenderer>();

        if (_sprite != null && !IsMarkerPaint)
        {
            _sprite.enabled = false;
        }

        _collider = GetComponent<BoxCollider2D>();

        _collider.size = _sprite.size;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        ITeamable unit = collision.GetComponentInParent<ITeamable>();

        if (unit is BattleBrain ready_unit)
        {
            Team playerTeam = GameController.Instance.PlayerTeam;
            Debug.Log($"Bank collision with unit: {collision.gameObject.name}");
            if (DamageSystem.Instance.Relationships.GetEnemyTeams(playerTeam).Contains(ready_unit.Team))
            {
                GameController.Instance?.LoseGame();
                GameController.Instance?.PauseGame();
            }
        }
    }
}