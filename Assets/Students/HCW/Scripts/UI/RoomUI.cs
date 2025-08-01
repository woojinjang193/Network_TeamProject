using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class RoomUI : BaseUI
{
    [Header("방 내부 패널")]
    [SerializeField] private TMP_Text roomNameText;
    [SerializeField] private Transform playerListContent; // 플레이어 목록이 들어갈 부모 Transform
    [SerializeField] private GameObject playerListItemPrefab; // 각 플레이어 정보를 표시할 UI 프리팹
    [SerializeField] private Button readyButton;
    [SerializeField] private Button startGameButton; // 방장만 활성화
    [SerializeField] private Button leaveRoomButton;

    [SerializeField] private RoomManager roomManager; // RoomManager 참조 추가

    [Header("팀 선택")]
    [SerializeField] private Button team1Button;
    [SerializeField] private Button team2Button;

    private void Awake()
    {
        Debug.Log("RoomUI: Awake 메서드 호출됨."); // 이 로그가 뜨는지 확인
        // 버튼 이벤트 연결
        readyButton.onClick.AddListener(OnReadyButtonClicked);
        startGameButton.onClick.AddListener(OnStartGameButtonClicked);
        leaveRoomButton.onClick.AddListener(OnLeaveRoomButtonClicked);

        team1Button.onClick.AddListener(() => OnClickChooseTeam("Team1"));
        team2Button.onClick.AddListener(() => OnClickChooseTeam("Team2"));
    }

    public override void Open()
    {
        gameObject.SetActive(true);
        // 방 이름 설정 (NetworkManager에서 호출 시 전달받거나 PhotonNetwork.CurrentRoom.Name 사용)
        if (Photon.Pun.PhotonNetwork.CurrentRoom != null)
        {
            roomNameText.text = Photon.Pun.PhotonNetwork.CurrentRoom.Name;
        }
        // TODO: 플레이어 목록 업데이트 로직 추가 (RoomManager와 연동)
    }

    public override void Close()
    {
        gameObject.SetActive(false);
    }

    private void OnReadyButtonClicked()
    {
        Debug.Log("준비 버튼 클릭됨");

        if (Photon.Pun.PhotonNetwork.LocalPlayer == null)
        {
            Debug.LogError("로컬 플레이어가 Photon 네트워크에 연결되지 않았습니다.");
            return;
        }

        // CustomProperties에서 "Ready" 속성 가져오기. 없으면 false로 초기화.
        object readyValue;
        bool currentReadyState = false;
        if (Photon.Pun.PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue("Ready", out readyValue))
        {
            currentReadyState = (bool)readyValue;
        }

        // 준비 상태 토글
        bool newReadyState = !currentReadyState;

        // CustomProperties 업데이트
        ExitGames.Client.Photon.Hashtable playerProps = new ExitGames.Client.Photon.Hashtable();
        playerProps.Add("Ready", newReadyState);
        Photon.Pun.PhotonNetwork.LocalPlayer.SetCustomProperties(playerProps);

        Debug.Log($"플레이어 {Photon.Pun.PhotonNetwork.LocalPlayer.NickName}의 준비 상태: {newReadyState}");
    }

    private void OnStartGameButtonClicked()
    {
        Debug.Log("게임 시작 버튼 클릭됨");
        if (roomManager != null)
        {
            roomManager.OnClickGameStart();
        }
    }

    private void OnLeaveRoomButtonClicked()
    {
        Debug.Log("방 나가기 버튼 클릭됨");
        NetworkManager.Instance.LeaveRoom();
    }

    public void UpdatePlayerList(List<Photon.Realtime.Player> players)
    {
        Debug.Log($"UpdatePlayerList 호출됨. 플레이어 수: {players.Count}");

        // 기존 플레이어 삭제
        foreach (Transform child in playerListContent)
        {
            Debug.Log($"기존 아이템 삭제: {child.name}");
            Destroy(child.gameObject);
        }

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
}
