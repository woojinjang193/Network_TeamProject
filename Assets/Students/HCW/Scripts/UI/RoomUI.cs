using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RoomUI : BaseUI
{
    [Header("방 내부 패널")]
    [SerializeField] private TMP_Text roomNameText;
    [SerializeField] public Transform playerListContent; // 플레이어 목록이 들어갈 부모 Transform
    [SerializeField] public GameObject playerListItemPrefab; // 각 플레이어 정보를 표시할 UI 프리팹
    [SerializeField] private Button readyButton;
    [SerializeField] private Button startGameButton; // 방장만 활성화
    [SerializeField] private Button leaveRoomButton;

    [SerializeField] private RoomManager roomManager; // RoomManager 참조 추가

    [Header("팀 선택")]
    [SerializeField] private Button team1Button;
    [SerializeField] private Button team2Button;

    [Header("봇 추가 및 제거")]
    [SerializeField] private Button addTeam1BotButton;
    [SerializeField] private Button addTeam2BotButton;
    [SerializeField] private Button clearBotsButton;

    [Header("맵 선택")]
    [SerializeField] private TMP_Dropdown mapDropdown;


    private void Awake()
    {
        Debug.Log("RoomUI: Awake 메서드 호출됨.");

        if (roomManager == null)
        {
            roomManager = FindObjectOfType<RoomManager>();
            if (roomManager == null)
            {
                Debug.LogError("RoomUI: RoomManager가 연결되지 않았습니다.");
            }
        }
        // 버튼 이벤트 연결
        readyButton.onClick.AddListener(OnReadyButtonClicked);
        startGameButton.onClick.AddListener(OnStartGameButtonClicked);
        leaveRoomButton.onClick.AddListener(OnLeaveRoomButtonClicked);
        //clearBotsButton.onClick.AddListener(OnClickClearBots);

        team1Button.onClick.AddListener(() => OnClickChooseTeam("Team1"));
        team2Button.onClick.AddListener(() => OnClickChooseTeam("Team2"));
        addTeam1BotButton.onClick.AddListener(() => roomManager.AddBot("Team1"));
        addTeam2BotButton.onClick.AddListener(() => roomManager.AddBot("Team2"));

        mapDropdown.options.Clear();
        mapDropdown.options.Add(new TMP_Dropdown.OptionData("Map1"));
        mapDropdown.options.Add(new TMP_Dropdown.OptionData("Map2"));
        mapDropdown.onValueChanged.AddListener(OnMapSelectionChanged);
    }


    public override void Open()
    {
        //RoomManager 참조 복구 (씬 복귀했을 때)
        if (roomManager == null)
        {
            roomManager = FindObjectOfType<RoomManager>();
            if (roomManager == null)
            {
                Debug.LogError("RoomManager를 찾을 수 없습니다.");
            }
        }
        if (!PhotonNetwork.InRoom)
        {
            Debug.LogWarning("RoomUI.Open(): 현재 방에 입장한 상태가 아님, UI 비활성화");
            return;
        }

        gameObject.SetActive(true);

        if (PhotonNetwork.CurrentRoom != null)
        {
            roomNameText.text = PhotonNetwork.CurrentRoom.Name;
            if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("map", out object mapName))
            {
                UpdateMapSelectionUI((string)mapName);
            }
        }
        addTeam1BotButton.interactable = PhotonNetwork.IsMasterClient;
        addTeam2BotButton.interactable = PhotonNetwork.IsMasterClient;
        mapDropdown.interactable = PhotonNetwork.IsMasterClient;
        roomManager?.PlayerPanelSpawnAll();
    }

    public override void Close()
    {
        gameObject.SetActive(false);
    }

    private void OnReadyButtonClicked()
    {
        Debug.Log("준비 버튼 클릭됨");

        if (PhotonNetwork.LocalPlayer == null)
        {
            Debug.LogError("로컬 플레이어가 Photon 네트워크에 연결되지 않았습니다.");
            return;
        }

        // CustomProperties에서 "Ready" 속성 가져오기. 없으면 false로 초기화.
        object readyValue;
        bool currentReadyState = false;
        if (PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue("Ready", out readyValue))
        {
            currentReadyState = (bool)readyValue;
        }

        // 준비 상태 토글
        bool newReadyState = !currentReadyState;

        // CustomProperties 업데이트
        ExitGames.Client.Photon.Hashtable playerProps = new ExitGames.Client.Photon.Hashtable()
        {
            ["Ready"] = newReadyState
        };
        PhotonNetwork.LocalPlayer.SetCustomProperties(playerProps);

        Debug.Log($"플레이어 {PhotonNetwork.LocalPlayer.NickName}의 준비 상태: {newReadyState}");
    }

    private void OnStartGameButtonClicked()
    {
        Debug.Log("게임 시작 버튼 클릭됨");
        if (roomManager != null)
        {
            roomManager.OnClickGameStart();
        }
    }
    
    // private void UpdateStartButton()
    // {
    //     startGameButton.gameObject.SetActive(PhotonNetwork.IsMasterClient);
    // }

    private void OnLeaveRoomButtonClicked()
    {
        Debug.Log("방 나가기 버튼 클릭됨");

        // ExitGames.Client.Photon.Hashtable resetProps = new()
        // {
        //     { "team", null },
        //     { "Ready", false }
        // };
        // PhotonNetwork.LocalPlayer.SetCustomProperties(resetProps);
        
        Manager.Net.roomManager?.ClearRoomData();
        Manager.Net.LeaveRoom();
    }

    public void ClearPlayerPanels()
    {
        // 기존 플레이어 및 봇 패널 모두 삭제
        foreach (Transform child in playerListContent)
        {
            Debug.Log($"기존 아이템 삭제: {child.name}");
            Destroy(child.gameObject);
        }
    }
    public void UpdatePlayerList(List<Player> players) // 받은 플레이어 리스트를 기준으로 패널 업데이트. 이후 봇 패널 생성
    {
        // 플레이어 및 봇 패널이 재생성 된다.
        Debug.Log($"UpdatePlayerList 호출됨. 플레이어 수: {players.Count}");

        ClearPlayerPanels();

        // 새 플레이어 생성 및 업데이트
        foreach (var player in players)
        {
            Debug.Log($"플레이어 {player.NickName} ({player.ActorNumber}) 아이템 생성 시도");
            GameObject itemGO = Instantiate(playerListItemPrefab, playerListContent);
            if (itemGO == null)
            {
                Debug.LogError("PlayerListItemPrefab이 null이거나 인스턴스화에 실패했습니다. RoomUI의 Player List Item Prefab 필드를 확인하세요.");
                continue;
            }
            Debug.Log($"아이템 생성 성공: {itemGO.name}");

            PlayerPanelItem item = itemGO.GetComponent<PlayerPanelItem>();
            if (item == null)
            {
                Debug.LogError($"생성된 아이템에 PlayerPanelItem 컴포넌트가 없습니다: {itemGO.name}");
                continue;
            }
            Debug.Log($"PlayerPanelItem 컴포넌트 찾음.");

            item.Init(player);
            item.ReadyCheck(player);
            item.ApplyTeamColor(player);
            Debug.Log($"플레이어 {player.NickName} ({player.ActorNumber}) 정보 초기화 완료.");
        }

        //봇 패널 재생성
        Debug.Log("봇 패널 재생성 시도");
        FindObjectOfType<RoomManager>()?.RefreshBotPanels();
    }

    public void SetStartButtonActive(bool isActive)
    {
        if (startGameButton != null)
        {
            startGameButton.gameObject.SetActive(isActive);
        }
    }

    public void OnClickChooseTeam(string team)
    {
        Debug.Log($"RoomUI: 팀 선택 버튼 클릭됨 - {team}");
        if (roomManager != null)
        {
            roomManager.OnClickChooseTeam(team);
        }
        else
        {
            Debug.LogError("RoomUI: RoomManager가 연결되지 않았습니다.");
        }
    }

    private void OnMapSelectionChanged(int index)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            string selectedMap = mapDropdown.options[index].text;
            roomManager.SetMapName(selectedMap);
        }
    }

    public void UpdateMapSelectionUI(string mapName) // 맵 변경 시 호출. 업데이트 한다.
    {
        if (mapName == "Map1")
        {
            mapDropdown.value = 0;
        }
        else
        {
            mapDropdown.value = 1;
        }
    }
    private void OnClickClearBots()
    {
        Debug.Log("Clear Bots 버튼 클릭됨");
        StartCoroutine(roomManager?.ClearBots());
    }
}
