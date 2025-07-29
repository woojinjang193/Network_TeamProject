using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro; 
using UnityEngine.SceneManagement;


public class NetworkManager : MonoBehaviourPunCallbacks
{
    public static NetworkManager Instance { get; private set; }

    [SerializeField] TMP_Text stateText;       // 네트워크 상태 표시용 텍스트
    [SerializeField] GameObject loadingPanel;  // 로딩 중일 때 보여줄 패널
    [SerializeField] GameObject lobbyPanel;    // 로비 화면 패널
    [SerializeField] GameObject loginPanel;    // 로그인 화면 패널
    [SerializeField] GameObject roomListItems;
    [SerializeField] LobbyPanel lobbyPanelScript;
    [SerializeField] GameObject roomPanel;     // 방 UI 패널

    [SerializeField] RoomManager roomManager; // 방 관리 스크립트


    private string _desiredRoomNameOnFail;     // 빠른 입장 실패 시 사용할 방 이름을 임시 저장하는 변수

    private void Awake()
    {
        Instance = this;
        Debug.Log("NetworkManager 초기화 완료");
    }

    private void Start()
    {
        if(SceneManager.GetActiveScene().name == "LoginScene")
        {
            ShowLoading(); // 로딩 화면 활성화
            PhotonNetwork.ConnectUsingSettings(); // Photon 서버에 연결 시도
            Debug.Log("Photon 서버 연결 시도 중...");
        }
        else
        {
            ShowLobby(); // 로비 화면 활성화
        }
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Photon 연결 완료");

        ShowLobby(); // 로비 화면 활성화
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        base.OnDisconnected(cause);
        Debug.LogWarning($"서버 연결 끊김: {cause}");

        // 다시 연결 시도
        PhotonNetwork.ConnectUsingSettings();
        Debug.Log("재연결 시도 중...");
    }

    private void Update()
    {
        stateText.text = $"State: {PhotonNetwork.NetworkClientState}";
    }
    public void ShowLoading()
    {
        Debug.Log("로딩 화면 활성화");
        loadingPanel.SetActive(true);
        lobbyPanel.SetActive(false);
    }

    public void CreateRoom(string roomName)
    {
        if (string.IsNullOrEmpty(roomName))
        {
            Debug.LogError("방 이름이 비어있음");
            return;
        }

        if (!PhotonNetwork.InLobby)
        {
            Debug.LogWarning("아직 로비에 참가하지 않음 → 로비 참가 후 생성");
            PhotonNetwork.JoinLobby(); //로비 참가부터 시도
            return;
        }

        RoomOptions options = new RoomOptions
        {
            MaxPlayers = 6,
            IsVisible = true,     //필수
            IsOpen = true         //입장 가능
        };

        PhotonNetwork.CreateRoom(roomName, options);
        Debug.Log($"방 생성 시도: {roomName}");
    }

    public void QuickJoinRoom(string desiredName)
    {
        _desiredRoomNameOnFail = string.IsNullOrEmpty(desiredName)
            ? $"Room_{Random.Range(1000, 9999)}"
            : desiredName;

        if (PhotonNetwork.IsConnectedAndReady)
        {
            PhotonNetwork.JoinRandomRoom();
        }
        else
        {
            PhotonNetwork.ConnectUsingSettings();
        }
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.LogError($"방 생성 실패: {message}");
    }

    public override void OnJoinedRoom()
    {
        lobbyPanel.SetActive(false);
        roomPanel.SetActive(true);

        roomManager.PlayerPanelspawn(); 
        Debug.Log("방 입장 성공!");
    }

    // 랜덤 입장에 실패했을 때 호출되는 콜백 (입장 가능한 방이 없을 때)
    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.LogWarning($"랜덤 입장 실패: {message} → 방 생성 시도");

        RoomOptions options = new RoomOptions { MaxPlayers = 6 };
        PhotonNetwork.CreateRoom(_desiredRoomNameOnFail, options);
    }

    public void ShowLobby()
    {
        loadingPanel.SetActive(false); // 로딩 꺼주고
        loginPanel.SetActive(false);   // 로그인 패널 끄고
        lobbyPanel.SetActive(true);    //로비 패널 켜기

        PhotonNetwork.JoinLobby();
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        lobbyPanelScript.UpdateRoomList(roomList); 
    }

    public void JoinRoom(string roomName)
    {
        if (string.IsNullOrEmpty(roomName))
        {
            Debug.LogError("입장할 방 이름이 비어있음");
            return;
        }

        PhotonNetwork.JoinRoom(roomName);
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)   // 새 플레이어가 현재 방에 입장했을 때 호출
    {
        if (newPlayer != PhotonNetwork.LocalPlayer)
            Debug.Log($"플레이어 입장: {newPlayer.NickName}");

        roomManager.PlayerPanelSpawn(newPlayer); // 새 플레이어 UI 생성
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        roomManager.PlayerPanelRemove(otherPlayer); 
    }


    public void LeaveRoom()   // 방 나가기 요청
    {
        PhotonNetwork.LeaveRoom();
    }

    public override void OnLeftRoom()  // 방 나간 후 처리
    {
        Debug.Log("방 나감,로비로 이동");
        roomManager.PlayerPanelRemove(PhotonNetwork.LocalPlayer); // 현재 플레이어 패널 제거
        roomPanel.SetActive(false);
        lobbyPanel.SetActive(true);
        lobbyPanelScript.OnReturnFromRoom();  // 로비 버튼 등 상태 복원
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        if (roomManager.playerPanels.TryGetValue(targetPlayer.ActorNumber, out var panel))
        {
            panel.ReadyCheck(targetPlayer); // 기존 Ready UI 반영
            panel.ApplyTeamColor(targetPlayer); // 팀 색상 반영
        }

        roomManager.CheckAllReady(); // 준비 여부 재확인
    }
}
