using ExitGames.Client.Photon; 
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using System.Linq; // Linq 네임스페이스 추가 
using UnityEngine;
using UnityEngine.UI;


public class RoomManager : MonoBehaviour
{
    [SerializeField] private RoomUI roomUI; // RoomUI 참조 추가

    public Dictionary<int, PlayerPanelItem> playerPanels = new Dictionary<int, PlayerPanelItem>();
    private bool isLoading;

    private int botCounter = 1;
    private List<PlayerPanelItem> botPanels = new();
    private List<BotInfo> botList = new();

    private void Awake()
    {
        if (Manager.Net.roomManager != null && Manager.Net.roomManager != this)
        {
            Destroy(gameObject);
            return;
        }

        Manager.Net.roomManager = this;
        DontDestroyOnLoad(gameObject);
    }

    /// 방 시작 버튼 클릭 시 호출
    public void OnClickGameStart()
    {
        if (!isLoading)
        {
            isLoading = true;
            PhotonNetwork.LoadLevel("Map2"); // 시작 시 GameScene 로드
        }
    }

    /// 새로운 플레이어가 들어왔을 때 패널 추가
    public void PlayerPanelSpawn(Player player)
    {
        playerPanels[player.ActorNumber] = null; // RoomUI가 UI를 관리하므로 null로 설정
        roomUI?.UpdatePlayerList(PhotonNetwork.PlayerList.ToList()); // RoomUI 업데이트
    }

    /// 현재 방의 모든 플레이어 패널 일괄 생성
    public void PlayerPanelSpawnAll()
    {
        playerPanels.Clear();
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            playerPanels[player.ActorNumber] = null; // RoomUI가 UI를 관리하므로 null로 설정
        }
        roomUI?.UpdatePlayerList(PhotonNetwork.PlayerList.ToList()); // RoomUI 업데이트
    }

    /// 플레이어가 나갔을 때 패널 제거
    public void PlayerPanelRemove(Player player)
    {
        if (playerPanels.ContainsKey(player.ActorNumber))
        {
            playerPanels.Remove(player.ActorNumber); // 딕셔너리에서 제거
        }
        roomUI?.UpdatePlayerList(PhotonNetwork.PlayerList.ToList()); // RoomUI 업데이트
    }

    /// 모든 플레이어가 준비 상태인지, 팀 배분이 올바른지 확인
    public void CheckAllReady()
    {
        int team1Count = 0, team2Count = 0;

        foreach (Player player in PhotonNetwork.PlayerList)
        {
            // Ready 체크
            if (!player.CustomProperties.TryGetValue("Ready", out object readyObj) || !(bool)readyObj)
            {
                roomUI?.SetStartButtonActive(false);
                return;
            }

            // 팀 체크
            if (player.CustomProperties.TryGetValue("team", out object teamObj))
            {
                string team = teamObj.ToString();
                if (team == "Team1") team1Count++;
                else if (team == "Team2") team2Count++;
            }
            else
            {
                // 팀이 안 정해진 경우도 실패 처리
                roomUI?.SetStartButtonActive(false);
                return;
            }
            //봇 상태 확인
            foreach (var bot in botList)
            {
                Debug.Log($"[CheckAllReady] 봇 {bot.Name} 준비 상태: {bot.IsReady}, 팀: {bot.Team}");

                if (!bot.IsReady)
                {
                    Debug.LogWarning($"[CheckAllReady] 봇 {bot.Name}이 준비되지 않음");
                    roomUI?.SetStartButtonActive(false);
                    return;
                }
                if (bot.Team == "Team1") team1Count++;
                else if (bot.Team == "Team2") team2Count++;
            }
            // 준비완료 && 팀 수 같음 && 최소 1명 이상일 때만 시작 버튼 활성화
            bool canStart = team1Count == team2Count && team1Count > 0;
            roomUI?.SetStartButtonActive(canStart && PhotonNetwork.IsMasterClient);
        }
    }

    /// 팀 선택 버튼 클릭 시 호출
    public void OnClickChooseTeam(string team)
    {
        Debug.Log($"RoomManager: 플레이어 {PhotonNetwork.LocalPlayer.NickName} 팀을 {team}으로 설정 시도");
        PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable
        {
            { "team", team },
            { "Ready", false }
        });
    }
    public void OnRoomJoined()
    {
        Debug.Log("RoomManager: 방 입장 처리 시작");

        if (roomUI == null)
            Debug.LogError("roomUI가 null입니다!");
        else
            Debug.Log("roomUI.Open() 호출");

        roomUI?.Open();
        PlayerPanelSpawnAll();
    }

    public void OnPlayerPropertiesUpdated(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        roomUI?.UpdatePlayerList(PhotonNetwork.PlayerList.ToList());
    }

    public void RoomReInit()
    {
        Debug.Log("[RoomUIInitializer] LoginScene 진입, UIManager 재초기화 시도");

        Manager.UI.Reinitialize();

        Manager.UI.ReplaceUI(typeof(RoomUI));

        roomUI = FindObjectOfType<RoomUI>(); // 꼭 다시 연결!

        if (roomUI == null)
            Debug.LogError("RoomReInit: RoomUI를 찾을 수 없습니다!");
        else
            Debug.Log("[RoomReInit] RoomUI 연결 성공");

        if (PhotonNetwork.InRoom)
        {
            Debug.Log("[RoomUIInitializer] PhotonNetwork.InRoom = true, RoomUI 활성화");

            OnRoomJoined();
        }
        else
        {
            Debug.Log("[RoomUIInitializer] 현재 방에 없음, LobbyUI 표시");
            Manager.UI.ReplaceUI(typeof(LobbyUI));
        }
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        isLoading = false;
    }
    public void AddBot(string team)
    {
        string botName = $"[BOT] Alpha{botCounter++}";

        BotInfo bot = new BotInfo
        {
            Name = botName,
            Team = team,
            IsReady = true
        };
        botList.Add(bot);

        GameObject panelGO = Instantiate(roomUI.playerListItemPrefab, roomUI.playerListContent);
        var panel = panelGO.GetComponent<PlayerPanelItem>();
        if (panel != null)
        {
            panel.InitBot(botName, team);
        }

        Debug.Log($"RoomManager: {team} 봇 추가됨 → {botName}");

        CheckAllReady();
    }
    public class BotInfo
    {
        public string Name;
        public string Team;
        public bool IsReady = true; // 항상 준비 상태로 시작
    }
    public void RefreshBotPanels()
    {
        if (roomUI == null || roomUI.playerListItemPrefab == null || roomUI.playerListContent == null)
        {
            Debug.LogWarning("RefreshBotPanels: RoomUI 또는 내부 필드가 null입니다.");
            return;
        }

        foreach (var bot in botList)
        {
            GameObject panelGO = Instantiate(roomUI.playerListItemPrefab, roomUI.playerListContent);
            var panel = panelGO.GetComponent<PlayerPanelItem>();
            if (panel != null)
            {
                panel.InitBot(bot.Name, bot.Team);
            }
        }
    }
    public List<BotInfo> GetBots()
    {
        return botList;
    }
}
