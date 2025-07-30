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

    [Header("방 생성")]
    [SerializeField] private TMP_InputField createRoomNameInput;

    

    

    private void Awake()
    {
        // 메인 로비 버튼 이벤트 연결
        createRoomButton.onClick.AddListener(OnCreateRoomButtonClicked);
        findRoomButton.onClick.AddListener(OnFindRoomButtonClicked);
        logoutButton.onClick.AddListener(OnLogoutButtonClicked);
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
    }

    

    private void OnCreateRoomButtonClicked()
    {
        string roomName = createRoomNameInput.text;
        if (string.IsNullOrEmpty(roomName))
        {
            Debug.Log("방 이름이 비어있어, 랜덤 방으로 생성합니다.");
            roomName = $"Room_{Random.Range(1000, 9999)}";
        }
        NetworkManager.Instance.CreateRoom(roomName);
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

    

    

    

    public void OnReturnFromRoom()
    {
        if (createRoomButton != null) createRoomButton.interactable = true;
    }
}
