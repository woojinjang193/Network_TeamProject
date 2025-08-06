using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using System.Linq; // Linq 네임스페이스 추가
using UnityEngine;
using UnityEngine.UI;
using Hashtable = ExitGames.Client.Photon.Hashtable;


public class RoomManager : MonoBehaviourPunCallbacks
{
    [SerializeField] private RoomUI roomUI; // RoomUI 참조 추가
    public string selectedMapName = "Map1"; // 기본 맵 설정

    public Dictionary<int, PlayerPanelItem> playerPanels = new Dictionary<int, PlayerPanelItem>();
    public Dictionary<int, PlayerPanelItem> botPanels = new Dictionary<int, PlayerPanelItem>();
    private bool isLoading;
    private int botNameCounter = 1;
    private int team1Counter = 0;
    private int team2Counter = 0;
    private int bot1Counter = 0; // 봇 전체 제거에 쓰임
    private int bot2Counter = 0;
    private int teamNoneCounter = 1;
    private void Awake(){}

    // 플레이어의 출입으로 패널 생성, 삭제 자동으로 되어 있음
    
    #region 콜백
    // 방 커스텀프로퍼티 변경 시 호출. 봇 생성, 맵 설정 때 호출 됨
    public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged) 
    {
        if (propertiesThatChanged.TryGetValue("map", out object mapName))
        {
            selectedMapName = (string)mapName;
            roomUI?.UpdateMapSelectionUI(selectedMapName); // 현재 맵 이름 기준으로 드롭다운 UI 최신화
        }
        if (propertiesThatChanged.ContainsKey("bots")) // 봇 키를 가지고 있을 경우
        {
            roomUI?.UpdatePlayerList(PhotonNetwork.PlayerList.ToList()); // 모든 플레이어의 패널을 업데이트(파괴 후 재생성)함
        }
        CheckAllReady();
    }
    
    // 플레이어 프로퍼티 변경 시 호출. 레디 상태 변경에 호출됨
    public void OnPlayerPropertiesUpdated(Player targetPlayer, Hashtable changedProps)
    {
        if (!PhotonNetwork.InRoom) return;
        roomUI?.UpdatePlayerList(PhotonNetwork.PlayerList.ToList());
        CheckAllReady();
        StartCoroutine(SetCountProperty());
    }
    
    #endregion
    
    #region 커스텀 프로퍼티 설정
    public void SetMapName(string mapName) // 맵 선택 드롭다운에 의해서 호출됨.
    {
        if (PhotonNetwork.IsMasterClient)
        {
            selectedMapName = mapName;
            // 맵 이름을 커스텀 프로퍼티로 저장함.
            PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable() { { "map", mapName } });
        }
    }
    public void OnClickChooseTeam(string team) // 플레이어 팀 변경 시 호출됨. 버튼에 의해서 호출
    {
        Debug.Log($"RoomManager: 플레이어 {PhotonNetwork.LocalPlayer.NickName} 팀을 {team}으로 설정 시도");
        // 현재 플레이어의 정보를 저장함
        PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable
        {
            { "team", team },
            { "Ready", false }
        });
    }
    
    public IEnumerator SetCountProperty()
    // CheckAllReady의 팀 카운트 결과 값을 커스텀 프로퍼티로 저장함
    {
        if (PhotonNetwork.IsMasterClient) // 외부에서도 보이도록 커스텀 프로퍼티 설정함
        {
            yield return null;
            CheckPlayerCount();
            Debug.Log($"커스텀 프로퍼티 설정 팀카운트{team1Counter} {team2Counter} {teamNoneCounter}");
            Hashtable teamCount = new()
            {
                ["t1C"] = team1Counter,
                ["t2C"] = team2Counter,
                ["tN"] = teamNoneCounter,
                ["tS"] = team1Counter + team2Counter + teamNoneCounter
            };
            PhotonNetwork.CurrentRoom.SetCustomProperties(teamCount);
        }
    }
    #endregion
    
    #region 커스텀 프로퍼티 체크
    public void CheckPlayerCount() // 현재 방의 플레이어 + 봇 숫자 체크 및 팀 체크
    {
        team1Counter = 0;
        team2Counter = 0;
        teamNoneCounter = 0;
        bot1Counter = 0; // 봇 전체 제거에 쓰임
        bot2Counter = 0;
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            // 팀 체크
            if (player.CustomProperties.TryGetValue("team", out object teamObj))
            {
                string team = teamObj.ToString();
                if (team == "Team1") team1Counter++;
                else if (team == "Team2") team2Counter++;
            }
            // 팀이 없는 경우
            else
            {
                teamNoneCounter++;
            }

            //봇 상태 확인
            if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("bots", out object botRaw))
            {
                var botList = botRaw as object[];
                foreach (var bot in botList)
                {
                    var b = bot as Hashtable;
                    if ((string)b["team"] == "Team1")
                    {
                        team1Counter++;
                        bot1Counter++;
                    }
                    else if ((string)b["team"] == "Team2")
                    {
                        team2Counter++;
                        bot2Counter++;
                    }
                    Debug.Log($" 봇 상태 확인 팀 1 카운트 : {team1Counter} 팀 2 카운트 :  {team2Counter}");
                }
            }
        }
    }
    
    public void CheckAllReady() // 모든 플레이어가 준비 됐는지 확인하고, 마스터클라이언트의 시작 버튼을 활성화함
    {
        bool isReady = true;

        Debug.Log("체크올레디 시작");
        int team1Count = 0, team2Count = 0, teamNone = 0;

        foreach (Player player in PhotonNetwork.PlayerList)
        {
            // Ready 체크
            if (!player.CustomProperties.TryGetValue("Ready", out object readyObj) || !(bool)readyObj)
            {
                roomUI?.SetStartButtonActive(false); // 레디 값을 찾을 수 없거나, 레디가 안됨
                isReady = false;
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
                teamNone++;
                roomUI?.SetStartButtonActive(false);
                isReady = false;
            }

            //봇 상태 확인
            if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("bots", out object botRaw))
            {
                var botList = botRaw as object[];
                foreach (var bot in botList)
                {
                    var b = bot as Hashtable;
                    if ((string)b["team"] == "Team1")
                    {
                        team1Count++;
                        Debug.Log($"팀1 카운트 ++ {team1Count}");
                    }
                    else if ((string)b["team"] == "Team2")
                    {
                        team2Count++;
                        Debug.Log($"팀2 카운트 ++ {team2Count}");
                    }
                }

                Debug.Log($" 봇 상태 확인 팀 1 카운트 : {team1Count} 팀 2 카운트 :  {team2Count}");
            }

            // 준비완료 && 팀 수 같음 && 최소 1명 이상일 때만 시작 버튼 활성화
            bool canStart = team1Count == team2Count && team1Count > 0 && teamNone != 0 && isReady;
            Debug.Log($"체크올레디 최종 확인 {team1Count} {team2Count} {teamNone}");
            roomUI?.SetStartButtonActive(canStart && PhotonNetwork.IsMasterClient);
        }
    }
    #endregion
    
    #region 게임 시작
    public void OnClickGameStart() // 게임 시작 버튼 누를 시. 게임시작 버튼 활성화는 필요 조건이 맞춰져야 함.(플레이어 수가 맞음 + 모든 플레이어 레디)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        if (!isLoading)
        {
            isLoading = true;
            PhotonNetwork.CurrentRoom.IsOpen = false;
            PhotonNetwork.CurrentRoom.IsVisible = false;
            // PhotonNetwork.AutomaticallySyncScene = true; // 마스터가 씬 전환 시 자동 동기화
            PhotonNetwork.LoadLevel(selectedMapName); // 시작 시 GameScene 로드
        }
    }
    #endregion

    #region 플레이어 패널 관리
    public void PlayerPanelSpawn(Player player)
    {
        //playerPanels[player.ActorNumber] = null; // RoomUI가 UI를 관리하므로 null로 설정
        roomUI?.UpdatePlayerList(PhotonNetwork.PlayerList.ToList()); // RoomUI 업데이트
    }

    /// 현재 방의 모든 플레이어 패널 일괄 생성
    public void PlayerPanelSpawnAll()
    {
        // playerPanels.Clear(); // 딕셔너리 클리어
        // foreach (Player player in PhotonNetwork.PlayerList)
        // {
        //     playerPanels[player.ActorNumber] = null; // 딕셔너리의 모든 값 제거
        // }
        roomUI?.UpdatePlayerList(PhotonNetwork.PlayerList.ToList()); // RoomUI 업데이트
    }

    /// 플레이어가 나갔을 때 패널 제거
    public void PlayerPanelRemove(Player player)
    {
        // if (playerPanels.ContainsKey(player.ActorNumber))
        // {
        //     playerPanels.Remove(player.ActorNumber); // 딕셔너리에서 제거
        // }
        roomUI?.UpdatePlayerList(PhotonNetwork.PlayerList.ToList()); // RoomUI 업데이트
    }
    #endregion
    
    #region 방 참가 및 UI 초기화
    public void OnRoomJoined()
    {
        roomUI?.Open();
        //PlayerPanelSpawnAll();
    }
    public void RoomReInit()
    {
        Debug.Log("RoomManager: RoomReInit 호출");

        Manager.UI.Reinitialize();
        Manager.UI.ReplaceUI(typeof(RoomUI));

        roomUI = FindObjectOfType<RoomUI>();
        roomUI?.Open();

        if (PhotonNetwork.InRoom)
            roomUI?.UpdatePlayerList(PhotonNetwork.PlayerList.ToList());
        else
            Manager.UI.ReplaceUI(typeof(LobbyUI));

        isLoading = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    public void ClearRoomData() // 방 정보 삭제
    {
        // 패널 초기화
        roomUI?.ClearPlayerPanels();
        botNameCounter = 1;
        team1Counter = 0;
        team2Counter = 0;
        teamNoneCounter = 0;
    }

    #endregion
    
    #region 봇관리
    public void AddBot(string team) // 봇 추가 버튼을 누르면 호출됨
    {
        if (!PhotonNetwork.IsMasterClient) return; // 마스터 클라이언트가 아니면 리턴

        CheckPlayerCount();
        int teamCountSum = team1Counter + team2Counter + teamNoneCounter;
        Debug.Log($"봇 생성 시도 : 팀1{team1Counter} 팀2{team2Counter} 합{teamCountSum}");
        
        if (teamCountSum>= 8)
        {
            Debug.Log("방 자리 꽉 참");
            return;
        }

        Debug.Log($"커스텀 프로퍼티 값 : {team1Counter} {team2Counter} {teamNoneCounter} {teamCountSum}");
        if (team == "Team1")
        {
            if (team1Counter >= 4)
            {
                Debug.Log("팀 1 자리 꽉참");
                return;
            }
            team1Counter++;
        }

        if (team == "Team2")
        {
            if (team2Counter >= 4)
            {
                Debug.Log("팀 2 자리 꽉참");
                return;
            }
            team2Counter++;
        }
        
        string botName = $"[BOT] Alpha{botNameCounter++}";
        Hashtable bot = new()
        {
            { "name", botName },
            { "team", team },
            { "isReady", true }
        };
        
        object[] bots = new object[] { bot };
        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("bots", out object existingBots))
        {
            List<object> newList = ((object[])existingBots).ToList();
            newList.Add(bot);
            bots = newList.ToArray();
        }

        Hashtable props = new Hashtable()
        {
            ["bots"] = bots,
            ["t1C"] = team1Counter,
            ["t2C"] = team2Counter,
            ["tN"] = teamNoneCounter,
            ["tS"] =  teamCountSum+1
        };
        PhotonNetwork.CurrentRoom.SetCustomProperties(props);
    }

    public void RefreshBotPanels() // 봇 패널 새로고침 함수. 봇 패널을 다시 생성한다. RoomUI의 UpdatePlayerList에서 호출된다
    {
        if (roomUI == null) return;

        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("bots", out object botsRaw)) // 봇 정보를 해시테이블의 리스트 형태로 가져옴
        {
            foreach (var obj in (object[])botsRaw)
            {
                var bot = obj as Hashtable;
                string name = (string)bot["name"];
                string team = (string)bot["team"];

                var panelGO = Instantiate(roomUI.playerListItemPrefab, roomUI.playerListContent);
                var panel = panelGO.GetComponent<PlayerPanelItem>();
                panel?.InitBot(name, team); // 봇의 정보를 기준으로 패널 초기화
            }
        }
    }
    public IEnumerator ClearBots() // 봇 전체 삭제
    {
        CheckPlayerCount(); // 현재 플레이어 카운트함
        
        team1Counter -= bot1Counter;
        team2Counter -= bot2Counter;
        
        Hashtable props = new Hashtable()
        {
            ["bots"] = new object[0],
            ["t1C"] = team1Counter,
            ["t2C"] = team2Counter,
            ["tN"] = teamNoneCounter,
            ["tS"] = team1Counter + team2Counter + teamNoneCounter
        };
        Debug.Log($"봇 제거후 값 저장 시도 {team1Counter} {team2Counter} {teamNoneCounter} {team1Counter + team2Counter + teamNoneCounter}");
        PhotonNetwork.CurrentRoom.SetCustomProperties(props);
        roomUI?.UpdatePlayerList(PhotonNetwork.PlayerList.ToList()); //봇이 전부 삭제된 상태로 플레이어 패널 최신화
        
        yield return null;
    }
    #endregion
}


