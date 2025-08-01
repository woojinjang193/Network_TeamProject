using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomAutoJoin : MonoBehaviourPunCallbacks
{
    private void Start()
    {
        if (string.IsNullOrEmpty(MatchData.LastRoomName))
            return;

        if (PhotonNetwork.IsConnectedAndReady)
        {
            TryJoinRoom();
        }
        else
        {
            PhotonNetwork.ConnectUsingSettings();
            PhotonNetwork.AutomaticallySyncScene = true;
            PhotonNetwork.NetworkingClient.StateChanged += OnPhotonConnected;
        }
    }

    private void OnPhotonConnected(ClientState previous, ClientState current)
    {
        if (current == ClientState.ConnectedToMasterServer)
        {
            PhotonNetwork.NetworkingClient.StateChanged -= OnPhotonConnected;
            TryJoinRoom();
        }
    }

    private void TryJoinRoom()
    {
        Debug.Log($"RoomAutoJoin: {MatchData.LastRoomName}로 재입장 시도");
        PhotonNetwork.JoinRoom(MatchData.LastRoomName);
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.LogWarning($"JoinRoom 실패: {message} → 방 새로 생성");
        RoomOptions options = new RoomOptions { MaxPlayers = 8 };
        PhotonNetwork.CreateRoom(MatchData.LastRoomName, options);
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("RoomAutoJoin: 기존 방 재입장 성공");
        UIManager.Instance.ReplaceUI(typeof(RoomUI));
    }
}
