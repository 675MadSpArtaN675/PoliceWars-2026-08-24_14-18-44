using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public struct TeamRelationship
{
    [SerializeField]
    public Team Team;
    [SerializeField]
    public List<Team> EnemyTeams;

    public override string ToString()
    {
        String out_str = "None";
        if (EnemyTeams.Count > 0)
        {
            out_str = EnemyTeams.Select((f) => f.ToString()).Aggregate((f, s) => f + ", " + s).ToString();
        }

        return $"Team: {Team}. Enemies: {out_str}";
    }
}