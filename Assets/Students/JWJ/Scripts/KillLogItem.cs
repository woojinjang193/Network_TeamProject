using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class KillLogItem : MonoBehaviour
{
    [SerializeField] private TMP_Text killerNameText;
    [SerializeField] private TMP_Text victimNameText;
    [SerializeField] private Image deathCauseImage;

    private TeamColorInfo teamColorInfo;

    private void Awake()
    {
        teamColorInfo = FindObjectOfType<TeamColorInfo>();
    }
    public void KillLogItemSet(string killer, string victim, Sprite causeSprite, int teamNum)
    {
        killerNameText.text = killer;
        victimNameText.text = victim;
        deathCauseImage.sprite = causeSprite;

        Team victimTeam = (Team)teamNum;

        Color myColor = teamColorInfo.GetTeamColor(victimTeam);
        Color killerTeam;

        if (victimTeam == Team.Team1)
        {
            killerTeam = teamColorInfo.GetTeamColor(Team.Team2);
        }
            
        else if (victimTeam == Team.Team2)
        {
            killerTeam = teamColorInfo.GetTeamColor(Team.Team1);
        }
            
        else //혹시 몰라서 
        {
            killerTeam = Color.gray;
        }

        killerNameText.color = killerTeam;
        victimNameText.color = myColor;
    }
}
