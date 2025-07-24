using Firebase.Auth;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyPanel : MonoBehaviour
{
    // 로그인 화면 패널
    [SerializeField] GameObject loginPanel;

    // 프로필 수정 화면 패널
    [SerializeField] GameObject editPanel;

    // 이메일, 닉네임, 유저ID 표시용 텍스트
    [SerializeField] TMP_Text emailText;
    [SerializeField] TMP_Text nameText;
    [SerializeField] TMP_Text userIdText;

    // 로그아웃, 프로필 수정, 방 생성, 빠른 입장 버튼
    [SerializeField] Button logoutButton;
    [SerializeField] Button editProfileButton;
    [SerializeField] Button createRoomButton;
    [SerializeField] Button quickJoinButton;

    // 방 이름 입력 필드
    [SerializeField] TMP_InputField roomNameField;

    private void Awake()
    {
        logoutButton.onClick.AddListener(Logout);// 로그아웃
        editProfileButton.onClick.AddListener(EditProfile);// 프로필 수정
        createRoomButton.onClick.AddListener(CreateRoom);// 방 생성
        quickJoinButton.onClick.AddListener(QuickJoin);// 빠른 입장
    }

    private void OnEnable()
    {
        // 로그인한 Firebase 유저 정보 받아오기
        FirebaseUser user = FirebaseManager.Auth.CurrentUser;

        // 이메일, 닉네임, 유저 ID UI에 표시
        emailText.text = user.Email;
        nameText.text = user.DisplayName;
        userIdText.text = user.UserId;
    }

    private void Logout()
    {
        FirebaseManager.Auth.SignOut(); // Firebase 로그아웃
        loginPanel.SetActive(true);     // 로그인 패널 활성화
        gameObject.SetActive(false);    // 현재 로비 패널 비활성화
    }

    private void EditProfile()
    {
        editPanel.SetActive(true);      // 프로필 수정 패널 활성화
        gameObject.SetActive(false);    // 현재 로비 패널 비활성화
    }

    // 입력된 방 이름으로 방 생성 요청
    private void CreateRoom()
    {
        createRoomButton.interactable = false;                   // 중복 클릭 방지
        NetworkManager.Instance.CreateRoom(roomNameField.text);  // 입력된 방 이름 전달
    }

    // 빠른 입장 시도 (입력된 방 이름도 전달)
    private void QuickJoin()
    {
        string desiredName = roomNameField.text;              // 방 이름 입력값 가져오기
        NetworkManager.Instance.QuickJoinRoom(desiredName);   // 네트워크 매니저에 전달
    }
}
