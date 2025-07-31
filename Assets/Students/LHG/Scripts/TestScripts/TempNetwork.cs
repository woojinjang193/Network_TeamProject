using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class TempNetwork : MonoBehaviourPunCallbacks
{
    void Start()
    {
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        //PhotonNetwork.JoinRandomOrCreateRoom(); //다른사람의 방에 들어갈 가능성 있음
        PhotonNetwork.JoinOrCreateRoom("LHK_AITEST", new RoomOptions(), TypedLobby.Default);  //별도 스트링 제목의 방을 만들어서(기본룸옵션관 로비타입으로)진행됨
    }

    public override void OnCreatedRoom()
    {
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("입장 완료");
        PhotonNetwork.LocalPlayer.NickName = $"Player_{PhotonNetwork.LocalPlayer.ActorNumber}";
        PlayerSpawn();
        AISpawn();
    }

    private void PlayerSpawn()
    {
        Vector3 spawnPos = new Vector3(Random.Range(0, 5), 1, Random.Range(0, 5));
        PhotonNetwork.Instantiate("Player1", spawnPos, Quaternion.identity);
    }
    public override void OnPlayerEnteredRoom(Player player)
    {
        Debug.Log($"{player.NickName} 입장 완료");
    }

    private void AISpawn()
    {
        Vector3 spawnPos = new Vector3(Random.Range(11, 16), 1, Random.Range(11, 16));
        PhotonNetwork.Instantiate("AI", spawnPos, Quaternion.identity);
    }
}
