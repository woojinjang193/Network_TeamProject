using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BotNameTag : MonoBehaviour
{
    private AIController aIController;

    private static List<string> botNames = new List<string>
    {
        "이학권의 작품","황천의 AI","이름뿐인 팀장 장우진","피카소", "반 고흐","먹다남은 치킨", 
        "봇이름 추천좀", "점심 뭐먹지", "zZ지존진영Zz", "민수민수최민수", "킹왕짱태우", "oO치우썬더Oo",
        "NullReferenceException", "김첨지", "플밍화이팅", "기능입니다", "봇아님", "마크 하실분"
    };
    private TMP_Text botNameText;
    private PhotonView photonView;
    Transform cam;

    private void Awake()
    {
        aIController = GetComponentInParent<AIController>();
        botNameText = GetComponentInChildren<TMP_Text>();
        photonView = GetComponent<PhotonView>();
        cam = Camera.main.transform;

    }

    void Start()
    {
        if (photonView.IsMine)
        {
            int i = Random.Range(0, botNames.Count);
            string botName = botNames[i];
            aIController.botName = botName; ///
            botNames.RemoveAt(i);

            photonView.RPC("SetBotName", RpcTarget.All, botName);
        }
    }

    [PunRPC]
    void SetBotName(string botName)
    {
        botNameText.text = botName;
    }

    private void LateUpdate()
    {
        Vector3 direction = transform.position - cam.position;
        transform.rotation = Quaternion.LookRotation(direction);
    }
}
