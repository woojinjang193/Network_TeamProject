using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RoomListUI : BaseUI
{
    [Header("방 리스트")]
    [SerializeField] private Transform roomListContent; // 스크롤뷰의 Content 넣기
    [SerializeField] private GameObject roomListItemPrefab; // 각 방 정보를 표시할 UI 프리팹
    [SerializeField] private Button backButton;
    //[SerializeField] private Button refreshButton; // 새로고침 버튼 추가
    
    private Dictionary<string, RoomListItemUI> roomListItems = new Dictionary<string, RoomListItemUI>();

    private UIManager uiManager;

    private void Awake()
    {
        backButton.onClick.AddListener(OnBackButtonClicked);
        //refreshButton.onClick.AddListener(OnRefreshButtonClicked); // 새로고침 버튼 리스너 추가
    }

    private void OnEnable()
    {
        Open();
        Debug.Log("오픈 들어옴");
        UpdateRoomList(Manager.Net.cachedRoomList);
        Debug.Log($"업데이트 룸리스트 돌아감 방 숫자:{Manager.Net.cachedRoomList.Count}");
    }
    public override void Open()
    {
        gameObject.SetActive(true);
    }

    public override void Close()
    {
        gameObject.SetActive(false);
    }

    private void OnBackButtonClicked()
    {
        Manager.UI.PopUI(); // 이전 UI (LobbyUI)로 돌아감
    }

    // private void OnRefreshButtonClicked()
    // {
    //     Manager.Net.RequestRoomListUpdate();
    // }

    public void UpdateRoomList(List<RoomInfo> roomList)
    {
        // 방 목록 UI를 업데이트하는 로직
        foreach (Transform child in roomListContent)
        {
            Destroy(child.gameObject);
        }
        roomListItems.Clear();

        foreach (var info in roomList)
        {
            Debug.Log($"방생성 로직에 들어옴 현재 방 이름:{info.Name} 공개여부: {info.RemovedFromList}");
            if (info.RemovedFromList) continue;

            GameObject itemGO = Instantiate(roomListItemPrefab, roomListContent);
            RoomListItemUI itemUI = itemGO.GetComponent<RoomListItemUI>();
            if (itemUI != null)
            {
                itemUI.Setup(info);
                Debug.Log($"방아이템 생성{info.Name}");
                roomListItems[info.Name] = itemUI;
            }
            
        }
    }

}
