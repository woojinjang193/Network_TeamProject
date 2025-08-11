using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BotNameTag : MonoBehaviour
{
    private AIController aIController;
    private TMP_Text botNameText;
    private PhotonView photonView;
    Transform cam;

    private void Awake()
    {
        aIController = GetComponentInParent<AIController>();
        botNameText = GetComponentInChildren<TMP_Text>();
        photonView = GetComponentInParent<PhotonView>();
        cam = Camera.main.transform;

    }

    void Start()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            int i = Random.Range(0, BotNameList.botNames.Count);
            string botName = BotNameList.botNames[i];
            aIController.botName = botName;
            BotNameList.botNames.RemoveAt(i);

            photonView.RPC("SetBotName", RpcTarget.AllBufferedViaServer, botName);
        }
    }

    [PunRPC]
    void SetBotName(string botName)
    {
        aIController.botName = botName;
        botNameText.text = botName;
    }

    private void LateUpdate()
    {
        Vector3 direction = transform.position - cam.position;
        transform.rotation = Quaternion.LookRotation(direction);
    }
}
