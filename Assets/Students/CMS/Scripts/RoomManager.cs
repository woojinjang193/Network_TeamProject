using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Realtime;
using ExitGames.Client.Photon; 
using System.Linq; // Linq 네임스페이스 추가 


public class RoomManager : MonoBehaviour
{
    [SerializeField] private RoomUI roomUI; // RoomUI 참조 추가

    [SerializeField] private int selectedMode = 2; // 1vs1 ~ 4vs4

    public Dictionary<int, PlayerPanelItem> playerPanels = new Dictionary<int, PlayerPanelItem>();

    private void Awake()
    {

    }

    /// 방 시작 버튼 클릭 시 호출
    public void OnClickGameStart()
    {
        PhotonNetwork.AutomaticallySyncScene = true; // 마스터가 씬 전환 시 자동 동기화
        PhotonNetwork.LoadLevel("JWJ_SampleScene"); // 시작 시 GameScene 로드
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
        }

        // 준비완료 && 팀 수 같음 && 최소 1명 이상일 때만 시작 버튼 활성화
        bool canStart = team1Count == team2Count && team1Count > 0;
        roomUI?.SetStartButtonActive(canStart && PhotonNetwork.IsMasterClient);
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
}
