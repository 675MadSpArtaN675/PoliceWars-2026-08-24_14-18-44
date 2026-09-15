using System;
using System.Collections.Generic;

using UnityEngine;

public class Deletable : MonoBehaviour
{
    public delegate void OnDeletedEvent(GameObject deleted_object);

    [InspectorName("Is can be deleted")]
    public bool IsCanDelete = true;

    [InspectorName("Only friendly deletable")]
    public bool IsFriendlyDeletable = true;

    public OnDeletedEvent OnDeleted;
    public Predicate<GameObject> DeletePredicate;

    private bool _is_entered = false;
    private ITeamable _teamable_component;

    private void Start()
    {
        _teamable_component = GetComponentInParent<ITeamable>();
    }

    private void OnMouseEnter()
    {
        _is_entered = true;
    }

    private void OnMouseExit()
    {
        _is_entered = false;
    }

    private void OnMouseDown()
    {
        if (DeletePredicate?.Invoke(gameObject) ?? true && IsCanDelete) {
            if (_is_entered && IsValidToDelete())
            {
                SelfDestruct();
            }
            else if (!IsFriendlyDeletable)
            {
                SelfDestruct();
            }
        }
    }

    public bool IsValidToDelete()
    {
        List<Team> _friend_teams = DamageSystem.Instance.Relationships.GetFriendTeams(GameController.Instance.PlayerTeam);

        return _teamable_component != null && (_teamable_component.Team == GameController.Instance.PlayerTeam || _friend_teams.Contains(_teamable_component.Team)) && IsFriendlyDeletable;
    }

    public void SelfDestruct()
    {
        if (GetComponentInParent<Projectile>() != null)
        {
            ProjectileSystem.Instance.DestroyProjectile(gameObject);
        }
        else if (IsValidToDelete())
        {
            PlantSystem.Instance.DeleteUnit(gameObject);
        }
        else
        {
            DeathSystem.Instance.DestroyUnit(gameObject);
        }
    }
}
