using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "TeamRelationships", menuName = "Scriptable Objects/TeamRelationships")]
public class TeamsRelationships : ScriptableObject
{
    private Dictionary<Team, List<Team>> _relationships;

    [InspectorName("Командные отношения")]
    public List<TeamRelationship> TeamRelationshipsList = new List<TeamRelationship>();

    public void OnEnable()
    {
        foreach (TeamRelationship team in TeamRelationshipsList)
        {
            Debug.Log($"Input team: {team}");
        }

        _relationships = TeamRelationshipsList.Select((TeamRelationship team_rel) =>
            new KeyValuePair<Team, List<Team>>(team_rel.Team,
            team_rel.EnemyTeams)
        )
        .ToDictionary(pair => pair.Key, pair => pair.Value);

        if (_relationships == null)
        {
            _relationships = new Dictionary<Team, List<Team>>();
        }
    }

    public List<Team> GetEnemyTeams(Team input_team)
    {
        if (_relationships != null) {
            Debug.Log($"Input team: {input_team}. List: {_relationships.Select((item) => item.ToString()).Aggregate((f, s) => f + ":"+ s)}");
            return _relationships.GetValueOrDefault(input_team, new List<Team>());
        }

        return new List<Team>();
    }

    public List<Team> GetFriendTeams(Team input_team)
    {
        if (_relationships.ContainsKey(input_team))
        {
            return GetTeams().Except(_relationships[input_team]).ToList();
        }

        return new List<Team>();
    }

    public List<Team> GetTeams()
    {
        return Enum.GetValues(typeof(Team)).Cast<Team>().ToList();
    }
}
