using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class TestNetworkManager_JYL : MonoBehaviourPunCallbacks
{
    void Start()
    {
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinOrCreateRoom("Test_Room_JYL",new RoomOptions(){MaxPlayers = 8},TypedLobby.Default);
    }

    public override void OnJoinedRoom()
    {
        PhotonNetwork.LocalPlayer.NickName = $"Player_{PhotonNetwork.LocalPlayer.ActorNumber}";
        PlayerSpawn();
    }

    private void PlayerSpawn()
    {
        PhotonNetwork.Instantiate("Player_CharacterTest", new Vector3(0, 5, 0), Quaternion.identity);
        PhotonNetwork.Instantiate("AI", new Vector3(1, 5, 1), Quaternion.identity);
    }
    public override void OnPlayerEnteredRoom(Player player)
    {
        base.OnPlayerEnteredRoom(player);
    }
}
