using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Realtime;
using ExitGames.Client.Photon; 


public class RoomManager : MonoBehaviour
{
    [SerializeField] private Button startButton; // 방 시작 버튼
    [SerializeField] private Button leaveButton; // 방 나가기 버튼
    [SerializeField] private Button team1Button;
    [SerializeField] private Button team2Button;

    [SerializeField] private GameObject playerPanelItemPrefabs;  // 플레이어 패널 프리팹
    [SerializeField] private Transform playerPanelContent;       // 플레이어 패널들이 추가될 부모 오브젝트

    public Dictionary<int, PlayerPanelItem> playerPanels = new Dictionary<int, PlayerPanelItem>();

    private void Awake()
    {
        leaveButton.onClick.AddListener(()=> NetworkManager.Instance.LeaveRoom()); // 방 나가기 버튼 클릭 시 호출
        startButton.onClick.AddListener(OnClickGameStart);

        team1Button.onClick.AddListener(() => OnClickChooseTeam("Team1"));
        team2Button.onClick.AddListener(() => OnClickChooseTeam("Team2"));
    }

    private void OnClickGameStart()
    {
        PhotonNetwork.LoadLevel("GameScene"); //시작시 GameScene 로드
    }

    public void PlayerPanelSpawn(Player player) // 새로운 플레이어가 들어왔을 때 패널 추가
    {
        GameObject obj = Instantiate(playerPanelItemPrefabs);      // 프리팹 생성
        obj.transform.SetParent(playerPanelContent);
        PlayerPanelItem item = obj.GetComponent<PlayerPanelItem>();
        //초기화
        item.Init(player);
        playerPanels.Add(player.ActorNumber, item);
    }

    public void PlayerPanelspawn()  // 현재 방의 모든 플레이어 패널 일괄 생성
    {
        foreach(Player player in PhotonNetwork.PlayerList)
        {
            GameObject obj = Instantiate(playerPanelItemPrefabs);
            obj.transform.SetParent(playerPanelContent);
            PlayerPanelItem item = obj.GetComponent<PlayerPanelItem>();
            //초기화
            item.Init(player);
            playerPanels.Add(player.ActorNumber, item);
        }
    }

    public void PlayerPanelRemove(Player player) // 플레이어가 나갔을 때 패널 제거
    {
        if (playerPanels.TryGetValue(player.ActorNumber, out PlayerPanelItem item))
        {
            Destroy(item.gameObject); // 패널 오브젝트 제거
            playerPanels.Remove(player.ActorNumber); // 딕셔너리에서 제거
        }
    }
    public void CheckAllReady()
    {
        int team1Count = 0, team2Count = 0;

        foreach (Player player in PhotonNetwork.PlayerList)
        {
            // Ready 체크
            if (!player.CustomProperties.TryGetValue("Ready", out object readyObj) || !(bool)readyObj)
            {
                SetStartButtonActive(false);
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
                SetStartButtonActive(false);
                return;
            }
        }

        // 준비완료 && 팀 수 같음 && 최소 1명 이상일 때만 시작 버튼 활성화
        bool canStart = team1Count == team2Count && team1Count > 0;
        SetStartButtonActive(canStart && PhotonNetwork.IsMasterClient);
    }

    private void SetStartButtonActive(bool isActive)
    {
        startButton.gameObject.SetActive(isActive);
    }
    public void OnClickChooseTeam(string team)
    {
        PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable
        {
            { "team", team },
            { "Ready", false }
        });
    }
}
