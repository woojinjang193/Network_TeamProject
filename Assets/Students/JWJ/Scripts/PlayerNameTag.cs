using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerNameTag : MonoBehaviourPun
{
    private TMP_Text nameText;
    Transform cam;

    private void Start()
    {
        nameText = GetComponentInChildren<TMP_Text>();
        cam = Camera.main.transform;
        nameText.text = photonView.Owner.NickName;

       //Debug.Log($"닉네임 세팅 : {nameText.text}");

        if(photonView.IsMine) //자신 닉네임 숨기기
        {
            nameText.text = null;
        }
    }
    private void LateUpdate()
    {
        Vector3 direction = transform.position - cam.position;
        transform.rotation = Quaternion.LookRotation(direction);
    }
}
