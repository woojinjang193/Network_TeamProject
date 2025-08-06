using ExitGames.Client.Photon; 
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class PlayerPanelItem : MonoBehaviour
{
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text readyText;
    [SerializeField] private Image hostImage;
    [SerializeField] private Image readyImage;
    [SerializeField] private Image backgroundImage; // 패널 배경

    public void Init(Player player)
    {
        nameText.text = player.NickName;
        hostImage.enabled = player.IsMasterClient; // 호스트 여부 표시

        ApplyTeamColor(player);

        ReadyCheck(player);
    }

    public void ReadyCheck(Player player)
    {
        bool isReady = false;
        if (player.CustomProperties.TryGetValue("Ready", out object value))
            isReady = (bool)value;

        readyText.text = isReady ? "Ready" : "Click Ready";
        readyImage.color = isReady ? Color.green : Color.white;
    }

    public void ApplyTeamColor(Player player)
    {
        if (player.CustomProperties.TryGetValue("team", out object team))
        {
            string teamName = team.ToString();
            if (teamName == "Team1")
                backgroundImage.color = new Color(106f, 0f, 255f); // 1팀 : 보라
            else if (teamName == "Team2")
                backgroundImage.color = new Color(243f, 255f, 0f); // 2팀 : 노랑
            else
                backgroundImage.color = Color.white;
        }
        else
        {
            backgroundImage.color = Color.white;
        }
    }
    public void InitBot(string botName, string team)
    {
        nameText.text = botName;
        readyText.text = "Ready";
        readyImage.color = Color.green; 
        hostImage.enabled = false;

        backgroundImage.color = team == "Team1"
            ? new Color(106f, 0f, 255f)
            : new Color(243f, 255f, 0f);
    }
}
