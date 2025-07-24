using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    public static NetworkManager Instance { get; private set; }

    [SerializeField] TMP_Text stateText;       // 네트워크 상태 표시용 텍스트
    [SerializeField] GameObject loadingPanel;  // 로딩 중일 때 보여줄 패널
    [SerializeField] GameObject lobbyPanel;    // 로비 화면 패널

    private string _desiredRoomNameOnFail;     // 빠른 입장 실패 시 사용할 방 이름을 임시 저장하는 변수

    private void Awake()
    {
        Instance = this;
    }

    public override void OnConnectedToMaster()
    {
        base.OnConnectedToMaster();
        Debug.Log("마스터 서버 연결 완료");

        loadingPanel.SetActive(false);
        lobbyPanel.SetActive(true);
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        base.OnDisconnected(cause);
        Debug.LogWarning($"서버 연결 끊김: {cause}");

        // 다시 연결 시도
        PhotonNetwork.ConnectUsingSettings();
    }


    private void Update()
    {
        stateText.text = $"State: {PhotonNetwork.NetworkClientState}";
    } 
    public void ShowLoading()
    {
        loadingPanel.SetActive(true);
        lobbyPanel.SetActive(false);
    }

    public void CreateRoom(string roomName)
    {
        // 방 이름이 비어있으면 생성하지 않음
        if (string.IsNullOrEmpty(roomName))
        {
            Debug.LogError("방 이름이 비어있음");
            return;
        }

        RoomOptions options = new RoomOptions { MaxPlayers = 6 }; // 방 옵션 설정 (최대 6인)
        PhotonNetwork.CreateRoom(roomName, options);
    }

    public void QuickJoinRoom(string desiredName)
    {
        PhotonNetwork.JoinRandomRoom();

        //랜덤 방 입장 실패 했을 때 사용할 방 이름
        // 만약 방 이름이 비어있다면 자동으로 생성
        _desiredRoomNameOnFail = string.IsNullOrEmpty(desiredName)
            ? $"Room_{Random.Range(1000, 9999)}"
            : desiredName;
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.LogError($"방 생성 실패: {message}");
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("방 입장 성공!");
        PhotonNetwork.LoadLevel("GameScene");//이건 게임 씬이 아닌 방 UI가 뜨게 할 것임
    }

    // 랜덤 입장에 실패했을 때 호출되는 콜백 (입장 가능한 방이 없을 때)
    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.LogWarning($"랜덤 입장 실패: {message} → 방 생성 시도");

        RoomOptions options = new RoomOptions { MaxPlayers = 6 };
        PhotonNetwork.CreateRoom(_desiredRoomNameOnFail, options);
    }
}
