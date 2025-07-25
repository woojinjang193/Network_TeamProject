using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PhotonConnectionTest : MonoBehaviourPunCallbacks
{
    private void Start()
    {
        Debug.Log("Connecting to Photon...");
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Photon 연결 성공! Region: " + PhotonNetwork.CloudRegion);
    }

    public override void OnDisconnected(Photon.Realtime.DisconnectCause cause)
    {
        Debug.LogError("Photon 연결 실패: " + cause);
    }
}
