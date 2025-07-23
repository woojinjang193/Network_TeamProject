using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Team
{
    None,
    Team1,
    Team2,
}
public class TeamColorInfo : MonoBehaviour
{
    [SerializeField] private Color team1Color;
    [SerializeField] private Color team2Color;

    public Color GetTeamColor(Team team)
    {
        if (team == Team.Team1)
        {
            return team1Color;
        }
        else if (team == Team.Team2)
        {
            return team2Color;
        }
        else
            return Color.gray;
    }
}
