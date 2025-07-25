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

    public Color Team1Color => team1Color;
    public Color Team2Color => team2Color;

    private Color team1InputColor = new Color(1f, 0f, 0f, 1f);
    private Color team2InputColor = new Color(0f, 1f, 0f, 1f);
    public Color Team1InputColor => team1InputColor;
    public Color Team2InputColor => team2InputColor;

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

    public Color GetTeamInputColor(Team team)
    {
        if (team == Team.Team1)
        {
            return team1InputColor;
        }
        else if (team == Team.Team2)
        {
            return Team2InputColor;
        }
        else
            return Color.gray;
    }
}
