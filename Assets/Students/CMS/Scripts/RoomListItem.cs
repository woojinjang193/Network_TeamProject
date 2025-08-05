using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RoomListItem : MonoBehaviourPun
{
    [SerializeField] private TMP_Text roomNameText;  // 방 이름 텍스트
    [SerializeField] private TMP_Text maxPlayerText; // 현재 인원 / 최대 인원 텍스트
    [SerializeField] private Button joinButton;
    private string roomName;

    public void Init(RoomInfo info)
    {
        roomName = info.Name;
        roomNameText.text = roomName; //UI에 이름 표시
        maxPlayerText.text = $"{info.PlayerCount} / {info.MaxPlayers}"; // 인원 표시

        GetComponent<Button>().onClick.AddListener(() =>  // 항목 클릭 시 방 입장 처리
        {
            Manager.Net.JoinRoom(roomName);  // 네트워크 매니저 통해 입장 요청
        });
    }

    public void JoinRoom()
    {
        if (PhotonNetwork.NetworkClientState == ClientState.JoinedLobby)
        {
            PhotonNetwork.JoinRoom(roomName);
            joinButton.onClick.RemoveAllListeners();
        }
    }
}
