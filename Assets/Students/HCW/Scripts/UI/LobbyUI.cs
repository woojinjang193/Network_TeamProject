using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// 로비 UI를 관리하는 스크립트
public class LobbyUI : BaseUI
{
    [Header("메인 로비 패널")]
    [SerializeField] private GameObject mainLobbyPanel;
    [SerializeField] private Button createRoomButton;
    [SerializeField] private Button findRoomButton;
    [SerializeField] private Button logoutButton;
    [SerializeField] private GameObject characterDisplayPanel; // 캐릭터를 보여줄 패널

    [Header("방 내부 패널")]
    [SerializeField] private GameObject roomPanel;
    [SerializeField] private TMP_Text roomNameText;
    [SerializeField] private Transform playerList;
    [SerializeField] private GameObject playerListPrefab; // 각 플레이어 정보를 표시할 UI 프리팹
    [SerializeField] private Button readyButton;
    [SerializeField] private Button startGameButton; // 방장만 활성화
    [SerializeField] private Button leaveRoomButton;

    private void Awake()
    {
        // 메인 로비 버튼 이벤트 연결
        createRoomButton.onClick.AddListener(OnCreateRoomButtonClicked);
        findRoomButton.onClick.AddListener(OnFindRoomButtonClicked);
        logoutButton.onClick.AddListener(OnLogoutButtonClicked);

        // 방 내부 버튼 이벤트 연결
        readyButton.onClick.AddListener(OnReadyButtonClicked);
        startGameButton.onClick.AddListener(OnStartGameButtonClicked);
        leaveRoomButton.onClick.AddListener(OnLeaveRoomButtonClicked);
    }

    public override void Open()
    {
        gameObject.SetActive(true);
        ShowMainLobby(); // 로비 UI가 열리면 기본적으로 메인 로비 화면을 보여줌
    }

    public override void Close()
    {
        gameObject.SetActive(false);
    }

    // 메인 로비 화면 활성화
    public void ShowMainLobby()
    {
        mainLobbyPanel.SetActive(true);
        roomPanel.SetActive(false);
    }

    // 방 내부 화면
    public void ShowRoomUI(string roomName = "")
    {
        mainLobbyPanel.SetActive(false);
        roomPanel.SetActive(true);
        roomNameText.text = roomName;
        // TODO: 플레이어 목록 업데이트 로직 추가
    }

    private void OnCreateRoomButtonClicked()
    {
        Debug.Log("방 생성 버튼 클릭됨");
        // TODO: 방 생성 로직 (Photon 연동)
        ShowRoomUI("새로운 방"); // 임시로 방 화면 보여주기
    }

    private void OnFindRoomButtonClicked()
    {
        UIManager.Instance.PushUI(typeof(RoomListUI)); // RoomListUI를 띄우도록 변경
    }

    private void OnLogoutButtonClicked()
    {
        Debug.Log("로그아웃 버튼 클릭됨");
        Firebase.Auth.FirebaseAuth.DefaultInstance.SignOut(); // Firebase 로그아웃 처리
        UIManager.Instance.ReplaceUI(typeof(LoginUI)); // 로그인 화면으로 돌아가기
    }

    private void OnReadyButtonClicked()
    {
        Debug.Log("준비 버튼 클릭됨");
        // TODO: 준비 상태 토글 로직 (Photon 연동)
    }

    private void OnStartGameButtonClicked()
    {
        Debug.Log("게임 시작 버튼 클릭됨");
        // TODO: 게임 시작 로직 (방장만, Photon 연동)
    }

    private void OnLeaveRoomButtonClicked()
    {
        Debug.Log("방 나가기 버튼 클릭됨");
        // TODO: 방 나가기 로직 (Photon 연동)
        ShowMainLobby(); // 메인 로비 화면으로 돌아가기
    }
}
