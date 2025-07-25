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
    // 로그인 화면으로 돌아갈 때 사용하는 로그인 패널
    [SerializeField] GameObject loginPanel;

    // 프로필 수정 시 보여지는 패널
    [SerializeField] GameObject editPanel;

    // 유저 정보 UI 요소들
    [SerializeField] TMP_Text emailText;
    [SerializeField] TMP_Text nameText;
    [SerializeField] TMP_Text userIdText;

    // 로비에서 사용하는 버튼
    [SerializeField] Button logoutButton;
    [SerializeField] Button editProfileButton;
    [SerializeField] Button createRoomButton;
    [SerializeField] Button quickJoinButton;

    // 방 이름 입력 필드
    [SerializeField] TMP_InputField roomNameField;

    // 방 목록에 사용될 프리팹과 부모 컨테이너
    [SerializeField] private GameObject roomListItemPrefabs;
    [SerializeField] private Transform roomListContent;

    // 현재 로비에 표시 중인 방 리스트 캐시
    private Dictionary<string, GameObject> roomListItems = new Dictionary<string, GameObject>();

    private void Awake()
    {
        // 버튼 클릭 시 연결될 이벤트 설정
        logoutButton.onClick.AddListener(Logout);
        editProfileButton.onClick.AddListener(EditProfile);
        createRoomButton.onClick.AddListener(CreateRoom);
        quickJoinButton.onClick.AddListener(QuickJoin);
    }

    private void OnEnable()
    {
        // Firebase에서 현재 로그인한 유저 정보 받아와서 UI에 표시
        FirebaseUser user = FirebaseManager.Auth.CurrentUser;
        emailText.text = user.Email;
        nameText.text = user.DisplayName;
        userIdText.text = user.UserId;
    }

    // 로그아웃 시 호출되는 함수
    private void Logout()
    {
        FirebaseManager.Auth.SignOut(); // Firebase 로그아웃 처리
        loginPanel.SetActive(true);     // 로그인 패널 다시 보여주기
        gameObject.SetActive(false);    // 현재 로비 패널은 숨기기
    }

    // 프로필 수정 버튼 클릭 시 호출
    private void EditProfile()
    {
        editPanel.SetActive(true);      // 프로필 수정 패널 켜기
        gameObject.SetActive(false);    // 로비 패널 끄기
    }

    // 방 생성 버튼 클릭 시 호출
    private void CreateRoom()
    {
        createRoomButton.interactable = false; // 중복 클릭 방지
        NetworkManager.Instance.CreateRoom(roomNameField.text); // 입력된 방 이름으로 방 생성 요청
    }

    // 빠른 입장 버튼 클릭 시 호출
    private void QuickJoin()
    {
        string desiredName = roomNameField.text; // 입력된 방 이름 값 읽기
        NetworkManager.Instance.QuickJoinRoom(desiredName); // 네트워크 매니저에 빠른 입장 요청
    }

    // 로비에서 수신한 방 목록을 UI에 업데이트
    public void UpdateRoomList(List<RoomInfo> roomList)
    {
        Debug.Log($"[RoomList] 업데이트 수신 - 방 수: {roomList.Count}");

        foreach (RoomInfo info in roomList)
        {
            // 삭제된 방이면 목록에서 제거
            if (info.RemovedFromList)
            {
                if (roomListItems.TryGetValue(info.Name, out GameObject obj))
                {
                    Destroy(obj);                   // 오브젝트 제거
                    roomListItems.Remove(info.Name); // 딕셔너리에서도 제거
                }
                continue;
            }
            // 이미 있는 방이면 정보만 업데이트
            if (roomListItems.ContainsKey(info.Name))
            {
                roomListItems[info.Name].GetComponent<RoomListItem>().Init(info);
            }
            else
            {
                // 새 방이면 프리팹을 인스턴스화해서 추가
                GameObject roomListItem = Instantiate(roomListItemPrefabs);
                roomListItem.transform.SetParent(roomListContent, false);
                roomListItem.GetComponent<RoomListItem>().Init(info);
                roomListItems.Add(info.Name, roomListItem);
            }
        }
    }

    // 룸에서 나와서 로비로 돌아왔을 때 호출되는 함수
    public void OnReturnFromRoom()
    {
        createRoomButton.interactable = true; // 방 생성 버튼 다시 활성화
    }
}
