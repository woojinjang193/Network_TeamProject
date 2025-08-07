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
        photonView = GetComponent<PhotonView>();
        cam = Camera.main.transform;

    }

    void Start()
    {
        if (photonView.IsMine)
        {
            int i = Random.Range(0, BotNameList.botNames.Count);
            string botName = BotNameList.botNames[i];
            aIController.botName = botName;
            BotNameList.botNames.RemoveAt(i);

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
