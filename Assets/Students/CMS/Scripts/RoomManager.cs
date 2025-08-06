using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using System.Linq; // Linq 네임스페이스 추가
using UnityEngine;
using UnityEngine.UI;


public class RoomManager : MonoBehaviourPunCallbacks
{
    [SerializeField] private RoomUI roomUI; // RoomUI 참조 추가
    public string selectedMapName = "Map1"; // 기본 맵 설정

    public Dictionary<int, PlayerPanelItem> playerPanels = new Dictionary<int, PlayerPanelItem>();
    private bool isLoading;
    private int botCounter = 1;
 
    private void Awake(){}

    #region 맵선택
    public void SetMapName(string mapName)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            selectedMapName = mapName;
            PhotonNetwork.CurrentRoom.SetCustomProperties(new ExitGames.Client.Photon.Hashtable() { { "map", mapName } });
        }
    }

    public override void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
    {
        if (propertiesThatChanged.TryGetValue("map", out object mapName))
        {
            selectedMapName = (string)mapName;
            roomUI?.UpdateMapSelectionUI(selectedMapName);
        }
        if (propertiesThatChanged.ContainsKey("bots"))
        {
            roomUI?.UpdatePlayerList(PhotonNetwork.PlayerList.ToList());
        }
    }
    #endregion

    #region 게임 시작
    public void OnClickGameStart()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        if (!isLoading)
        {
            isLoading = true;
            // PhotonNetwork.AutomaticallySyncScene = true; // 마스터가 씬 전환 시 자동 동기화
            PhotonNetwork.LoadLevel(selectedMapName); // 시작 시 GameScene 로드
        }
    }
    #endregion

    #region 플레이어 패널 관리
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
    #endregion

    #region 플레이어 커스텀프로퍼티 업데이트
    public void OnClickChooseTeam(string team)
    {
        Debug.Log($"RoomManager: 플레이어 {PhotonNetwork.LocalPlayer.NickName} 팀을 {team}으로 설정 시도");
        PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable
        {
            { "team", team },
            { "Ready", false }
        });
    } 
    public void OnPlayerPropertiesUpdated(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        roomUI?.UpdatePlayerList(PhotonNetwork.PlayerList.ToList());
    }
   
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
            if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("bots", out object botRaw))
            {
                var botList = botRaw as object[];
                foreach (var bot in botList)
                {
                    var b = bot as ExitGames.Client.Photon.Hashtable;
                    if ((string)b["team"] == "Team1") team1Count++;
                    else if ((string)b["team"] == "Team2") team2Count++;
                }
            }
            // 준비완료 && 팀 수 같음 && 최소 1명 이상일 때만 시작 버튼 활성화
            bool canStart = team1Count == team2Count && team1Count > 0;
            roomUI?.SetStartButtonActive(canStart && PhotonNetwork.IsMasterClient);
        }
    }
    #endregion

    #region 봇관리
    public void AddBot(string team)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        string botName = $"[BOT] Alpha{botCounter++}";
        ExitGames.Client.Photon.Hashtable bot = new()
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

        PhotonNetwork.CurrentRoom.SetCustomProperties(new ExitGames.Client.Photon.Hashtable { { "bots", bots } });

        CheckAllReady();
    }

    public void RefreshBotPanels()
    {
        if (roomUI == null) return;

        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("bots", out object botsRaw))
        {
            foreach (var obj in (object[])botsRaw)
            {
                var bot = obj as ExitGames.Client.Photon.Hashtable;
                string name = (string)bot["name"];
                string team = (string)bot["team"];

                var panelGO = Instantiate(roomUI.playerListItemPrefab, roomUI.playerListContent);
                var panel = panelGO.GetComponent<PlayerPanelItem>();
                panel?.InitBot(name, team);
            }
        }
    }
    public void ClearBots()
    {
        PhotonNetwork.CurrentRoom.SetCustomProperties(new ExitGames.Client.Photon.Hashtable { { "bots", new object[0] } });
        roomUI?.UpdatePlayerList(PhotonNetwork.PlayerList.ToList());
    }
    public void ClearRoomData()
    {
        // 봇 초기화
        PhotonNetwork.CurrentRoom?.SetCustomProperties(new ExitGames.Client.Photon.Hashtable { { "bots", new object[0] } });

        // 패널 초기화
        roomUI?.UpdatePlayerList(PhotonNetwork.PlayerList.ToList());
        botCounter = 1;
    }
    #endregion
}

