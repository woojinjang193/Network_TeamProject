using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class KillBoard : MonoBehaviour
{
    private PhotonView killLogView;

    [Header("당사자용")]
    [SerializeField] private GameObject indiKillLog;
    [SerializeField] private TMP_Text killText;

    [Header("모든 플레이어용 이미지")]
    [SerializeField] private Sprite killByPlayerImage;
    [SerializeField] private Sprite killByBotImage;
    [SerializeField] private Sprite killByInkImage;
    [SerializeField] private Sprite killByFallImage;

    [Header("소환위치, 프리팹")]
    [SerializeField] private Transform killLogPanel;
    [SerializeField] private GameObject KillLogItemPrefab;



    private void Awake()
    {
        killLogView = GetComponent<PhotonView>();
        indiKillLog.SetActive(false);
    }

    public void KillLog(string text)
    {
        indiKillLog.SetActive(false);
        indiKillLog.SetActive(true);
        killText.text = text;
    }

    [PunRPC]
    public void LogForAll(string killerName, string victimName, int causeNum)
    {
        DeathCause cause = (DeathCause)causeNum;
        Sprite causeSprite = null;

        switch (cause)
        {
            case DeathCause.PlayerAttack:
                causeSprite = killByPlayerImage;
                break;

            case DeathCause.BotAttck:
                causeSprite = killByBotImage;
                break;

            case DeathCause.Fall:
                killerName = "";
                causeSprite = killByFallImage;
                break;

            case DeathCause.EnemyInk:
                killerName = "";
                causeSprite = killByInkImage;
                break;
        }

        if (killLogPanel.childCount >= 3)
        {
            Destroy(killLogPanel.GetChild(0).gameObject);
        }

        GameObject killLogItem = Instantiate(KillLogItemPrefab, killLogPanel);

        KillLogItem item = killLogItem.GetComponent<KillLogItem>();

        item.KillLogItemSet(killerName, victimName, causeSprite);

        Destroy(killLogItem, 5f);

    }

}
