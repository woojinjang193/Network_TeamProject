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
    [SerializeField] RoomManager roomManager; // 방 관리 스크립트


    private string _desiredRoomNameOnFail;     // 빠른 입장 실패 시 사용할 방 이름을 임시 저장하는 변수
    public List<RoomInfo> cachedRoomList =  new List<RoomInfo>();

    private void Awake()
    {
        Instance = this;
        Debug.Log("NetworkManager 초기화 완료");
    }

    private void Start()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.ConnectUsingSettings();
        Debug.Log("Photon 서버 연결 시도 중...");
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("마스터 서버 재연결 완료, 방 재입장 시도");

        if (!string.IsNullOrEmpty(MatchData.LastRoomName))
        {
            PhotonNetwork.JoinRoom(MatchData.LastRoomName);
        }
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        base.OnDisconnected(cause);
        Debug.LogWarning($"서버 연결 끊김: {cause}");

        // 다시 연결 시도
        PhotonNetwork.ConnectUsingSettings();
        Debug.Log("재연결 시도 중...");
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
            IsVisible = true,
            IsOpen = true
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
        Debug.Log("방 입장 성공!");

        // UIManager를 통해 RoomUI를 표시
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ReplaceUI(typeof(RoomUI));
        }

        // RoomManager를 통해 플레이어 목록 생성
        if (roomManager != null) 
        {
            roomManager.PlayerPanelSpawnAll();
        } 
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
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ReplaceUI(typeof(LobbyUI));
        }
        PhotonNetwork.JoinLobby();
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        cachedRoomList.Clear();
        cachedRoomList = roomList;
        
        if (UIManager.Instance != null && UIManager.Instance.CurrentUI is RoomListUI roomListUI)
        {
            roomListUI.UpdateRoomList(roomList);
            Debug.Log("룸리스트 UI 만드는 함수 수행들어감OnRoomListUpdate");
        }
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

    public override void OnLeftRoom()
    {
        Debug.Log("방 나감, 로비로 이동");

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ReplaceUI(typeof(LobbyUI));

            StartCoroutine(WaitAndCallLobbyUI());
        }

        if (roomManager != null)
        {
            roomManager.PlayerPanelRemove(PhotonNetwork.LocalPlayer);
        }
    }

    private IEnumerator WaitAndCallLobbyUI()
    {
        // LobbyUI가 생성될 때까지 대기 (최대 1초 정도)
        float timeout = 1f;
        float timer = 0f;

        while (!(UIManager.Instance.CurrentUI is LobbyUI) && timer < timeout)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        if (UIManager.Instance.CurrentUI is LobbyUI lobbyUI)
        {
            lobbyUI.ShowMainLobby();
            lobbyUI.OnReturnFromRoom();
        }
        else
        {
            Debug.LogWarning("LobbyUI에 접근 실패: 시간 초과 또는 UI 생성 실패");
        }
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        roomManager?.OnPlayerPropertiesUpdated(targetPlayer, changedProps);

        roomManager?.CheckAllReady();
    }
}
