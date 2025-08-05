using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro; 
using UnityEngine;
using UnityEngine.SceneManagement;


public class NetworkManager : SingletonPunCallbacks<NetworkManager>
{
    public RoomManager roomManager;

    public List<RoomInfo> cachedRoomList = new();

    private string _desiredRoomNameOnFail; // 빠른 입장 실패 시 사용할 방 이름을 임시 저장하는 변수

    protected override void Awake()
    {
        base.Awake();
        roomManager = FindObjectOfType<RoomManager>();
        Debug.Log("NetworkManager 초기화 완료");
        
    }


    private void Start()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
    }
    public override void OnConnectedToMaster()
    {
        Debug.Log("Photon 연결 완료");

        if (PhotonNetwork.InRoom)
        {
            Debug.Log("[NetworkManager] 이미 방에 있음,LobbyUI 띄우기");
            ShowLobby();
        }
        else
        {
            ShowLobby();
        }
    }


    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.LogWarning($"서버 연결 끊김: {cause}");
        PhotonNetwork.ConnectUsingSettings();
    }

    public void CreateRoom(string roomName)
    {
        if (string.IsNullOrEmpty(roomName))
        {
            Debug.LogError("방 이름 없음");
            return;
        }

        if (!PhotonNetwork.InLobby)
        {
            PhotonNetwork.JoinLobby(); //로비 참가부터 시도
            return;
        }

        RoomOptions options = new RoomOptions { MaxPlayers = 6, IsVisible = true, IsOpen = true };
        PhotonNetwork.CreateRoom(roomName, options);
    }

    public void QuickJoinRoom(string desiredName)
    {
        _desiredRoomNameOnFail = string.IsNullOrEmpty(desiredName)
            ? $"Room_{Random.Range(1000, 9999)}"
            : desiredName;

        if (PhotonNetwork.IsConnectedAndReady)
            PhotonNetwork.JoinRandomRoom();
        else
            PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.LogError($"방 생성 실패: {message}");
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("방 입장 성공!");

        roomManager?.OnRoomJoined();
        Manager.UI.ReplaceUI(typeof(LobbyUI));
    }


    // 랜덤 입장에 실패했을 때 호출되는 콜백 (입장 가능한 방이 없을 때)
    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.LogWarning($"랜덤 입장 실패 생성: {message}");

        RoomOptions options = new RoomOptions { MaxPlayers = 6 };
        PhotonNetwork.CreateRoom(_desiredRoomNameOnFail, options);
    }

    public void ShowLobby()
    {
        Manager.UI.ReplaceUI(typeof(LobbyUI));
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        cachedRoomList = roomList;

        Debug.Log($"[NetworkManager] RoomList 업데이트 수신됨, 방 수: {roomList.Count}");

        if (Manager.UI.CurrentUI is RoomListUI roomListUI)
        {
            roomListUI.UpdateRoomList(roomList);
            Debug.Log("룸리스트 UI 업데이트 실행");
        }
        foreach (var room in roomList)
        {
            Debug.Log($"방 이름 확인: {room.Name}");
        }
    }

    public void JoinRoom(string roomName)
    {
        if (PhotonNetwork.IsConnectedAndReady)
        {
            PhotonNetwork.JoinRoom(roomName);
        }
        else
        {
            Debug.LogWarning("Photon 준비 안 됨 → JoinRoom 실패");
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer) // 새 플레이어가 현재 방에 입장했을 때 호출
    {
        if (newPlayer != PhotonNetwork.LocalPlayer)
            Debug.Log($"플레이어 입장: {newPlayer.NickName}");

        roomManager?.PlayerPanelSpawn(newPlayer);
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        roomManager?.PlayerPanelRemove(otherPlayer);
    }

    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
    }

    public override void OnLeftRoom()
    {
        Debug.Log("OnLeftRoom 호출됨");

        if (Manager.UI.CurrentUI != null)
        {
            Debug.Log($"현재 UI 타입: {Manager.UI.CurrentUI.GetType().Name}");
        }
        else
        {
            Debug.Log("CurrentUI가 null임");
        }

        var allUIs = GameObject.FindObjectsOfType<RoomUI>(true);
        foreach (var roomUI in allUIs)
        {
            roomUI.Close();
        }

        Manager.UI.ReplaceUI(typeof(LobbyUI));
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("로비 입장 완료");
            ShowLobby(); 
    }
    // 이전 방으로 돌아가기
    public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        roomManager?.OnPlayerPropertiesUpdated(targetPlayer, changedProps);
        roomManager?.CheckAllReady();
    }
    // JoinRoom 실패 시 콜백
    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.LogWarning($"JoinRoom 실패: {message}");
    }
}
