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

    private Coroutine waitRoutine;
    
    #region 유니티함수
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
    #endregion
    
    #region 서버 연결
    public override void OnConnectedToMaster() // 마스터 서버 연결
    {
        if (PhotonNetwork.NetworkClientState == ClientState.ConnectedToMasterServer)
        {
            Debug.Log("Photon 연결 완료");

            // if (PhotonNetwork.InRoom)
            // {
            //     Debug.Log("[NetworkManager] 이미 방에 있음,LobbyUI 띄우기");
            //     ShowLobby();
            // }
            // TODO : 로비쪽에서 구현 필요
            
            PhotonNetwork.JoinLobby();
        }
    }
    
    public override void OnDisconnected(DisconnectCause cause) // 연결이 끊어 졌을 때
    {
        Debug.LogWarning($"서버 연결 끊김: {cause}");
        PhotonNetwork.ConnectUsingSettings();
    }
    #endregion
    
    #region 로비 연결, 방목록
    public override void OnJoinedLobby() //로비에 들어갔을 때
    {
        Debug.Log("로비 입장 완료");
        Manager.UI.ReplaceUI(typeof(LobbyUI)); // 로비 UI 띄움
    }
    
    public override void OnRoomListUpdate(List<RoomInfo> roomList) // 방 목록 정보에 변경이 있었을 때
    {
        cachedRoomList = roomList; // 방 목록을 캐싱함

        Debug.Log($"[NetworkManager] RoomList 업데이트 수신됨, 방 수: {roomList.Count}");

        if (Manager.UI.CurrentUI is RoomListUI roomListUI) // 현재 띄워지고 있는 UI가 방목록이면
        {
            roomListUI.UpdateRoomList(roomList); // 방 목록 업데이트함
            Debug.Log("룸리스트 UI 업데이트 실행");
        }
        foreach (var room in roomList) // 방 정보들을 순회
        {
            Debug.Log($"방 이름 확인: {room.Name}");
        }
    }
    
    // public void RequestRoomListUpdate() // 로비 다시 참가 요청
    // {
    //     if (PhotonNetwork.NetworkClientState == ClientState.JoinedLobby) // 로비 상태일 때만 사용
    //     {
    //         PhotonNetwork.JoinLobby(); // 로비에 다시 조인하여 방 목록 업데이트 요청
    //         Debug.Log("방 목록 업데이트 요청");
    //     }
    //     else
    //     {
    //         Debug.LogWarning($"로비 재참가 요청 실패{PhotonNetwork.NetworkClientState}");
    //     }
    //
    //     Manager.UI.ReplaceUI(typeof(RoomListUI));
    // }
    #endregion

    #region 방 생성
    public void CreateRoom(string roomName) // 방을 만들 때 사용하는 함수
    {
        if (PhotonNetwork.NetworkClientState != ClientState.JoinedLobby)
        {
            PhotonNetwork.JoinLobby(); //로비 참가부터 시도
        }

        if (waitRoutine == null)
        {
            waitRoutine = StartCoroutine(CreateRoomRoutine(roomName)); // 방 생성 코루틴
        }
        else
        {
            Debug.Log("방 생성 대기중");
        }

    }
    
    private IEnumerator CreateRoomRoutine(string roomName) // 방 생성 시도 코루틴
    {
        while (true)
        {
            if (PhotonNetwork.NetworkClientState == ClientState.JoinedLobby) // 로비에 접속 해야만 방 생성
            {
                        
                RoomOptions options = new RoomOptions { MaxPlayers = 8, IsVisible = true, IsOpen = true };
        
                PhotonNetwork.CreateRoom(roomName, options); // 방 생성
                if (waitRoutine != null)
                {
                    StopCoroutine(waitRoutine);
                    waitRoutine = null;
                    
                }
                yield break;
            }
            yield return new WaitForSecondsRealtime(0.5f);
        }
    }
    public override void OnCreateRoomFailed(short returnCode, string message) // 룸 생성 실패
    {
        Debug.LogWarning($"방 생성 실패: {message}");
    }
    #endregion
    
    #region 방 안에서 업데이트
    // 플레이어 정보가 업데이트 됐을 때
    public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        roomManager?.OnPlayerPropertiesUpdated(targetPlayer, changedProps);
        roomManager?.CheckAllReady();
    }
    public override void OnPlayerEnteredRoom(Player newPlayer) // 새 플레이어가 현재 방에 입장했을 때 호출
    {
        if (newPlayer != PhotonNetwork.LocalPlayer)
            Debug.Log($"플레이어 입장: {newPlayer.NickName}");

        roomManager?.PlayerPanelSpawn(newPlayer);
    }
    public override void OnPlayerLeftRoom(Player otherPlayer) // 현재 방에서 다른 플레이어가 떠났을 때 호출
    {
        roomManager?.PlayerPanelRemove(otherPlayer);
    }
    #endregion
    
    #region 방 나갈 때
    public void LeaveRoom() // 방을 떠날 때 사용하는 함수
    {
        PhotonNetwork.LeaveRoom(); // OnLeftRoom 호출 됨
    }
    public override void OnLeftRoom() // 방을 떠날 때 호출
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

        var allUIs = FindObjectsOfType<RoomUI>(true);
        foreach (var roomUI in allUIs)
        {
            roomUI.Close();
        }
        PhotonNetwork.JoinLobby(); //방을 떠날 때, 로비에 재참가
    }

    #endregion

    #region 방 참가
    public override void OnJoinedRoom() // 방 참가 시 호출
    {
        Debug.Log("방 입장 성공!");

        Manager.UI.ReplaceUI(typeof(RoomUI));
        roomManager?.OnRoomJoined();
    }
    public void JoinRoom(string roomName) // 방에 참가할 때 사용되는 함수
    {
        if (PhotonNetwork.NetworkClientState == ClientState.JoinedLobby)
        {
            PhotonNetwork.JoinRoom(roomName);
        }
        else
        {
            Debug.LogWarning("Photon 준비 안 됨 → JoinRoom 실패");
        }
    }
    // JoinRoom 실패 시 콜백
    public override void OnJoinRoomFailed(short returnCode, string message) // 방 참가 실패
    {
        Debug.LogWarning($"JoinRoom 실패: {message}");
    }
    #endregion
    // public void QuickJoinRoom(string desiredName) // 빠른 참가
    // {
    //     _desiredRoomNameOnFail = string.IsNullOrEmpty(desiredName)
    //         ? $"Room_{Random.Range(1000, 9999)}"
    //         : desiredName; // 죽은코드
    //
    //     if (PhotonNetwork.IsConnectedAndReady)
    //         PhotonNetwork.JoinRandomRoom();
    //     else
    //         PhotonNetwork.ConnectUsingSettings();
    // }
    // // 랜덤 입장에 실패했을 때 호출되는 콜백 (입장 가능한 방이 없을 때)
    // public override void OnJoinRandomFailed(short returnCode, string message)
    // {
    //     Debug.LogWarning($"랜덤 입장 실패 생성: {message}");
    //
    //     RoomOptions options = new RoomOptions { MaxPlayers = 6 };
    //     PhotonNetwork.CreateRoom(_desiredRoomNameOnFail, options);
    // }
}
