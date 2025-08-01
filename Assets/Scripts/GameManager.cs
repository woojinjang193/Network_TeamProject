using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : Singleton<GameManager>
{
    public bool IsGameEnd {  get; private set; }

    private Dictionary<Collider, BaseController> playerDic = new();
    //플레이어의 콜라이더와 컨트롤러를 넣을 딕셔너리 

    [Header("팀별 스폰 위치")]
    [SerializeField] private Transform[] team1SpawnPoints;   // Team1 스폰 
    [SerializeField] private Transform[] team2SpawnPoints;   // Team2 스폰 
    [SerializeField] private string playerPrefabName = "Player_CharacterTest";

    private double startTime;
    [SerializeField] private float matchDuration = 180f;
    [SerializeField] private TMP_Text timerText;
    private PhotonView photonView;

    protected override void Awake()
    {
        base.Awake();
        photonView = GetComponent<PhotonView>();
        DontDestroyOnLoad(gameObject);
    }
    private void Start()
    {
        Debug.Log("=== [GameManager] Start ===");

        Debug.Log("[GameManager] LocalPlayer CustomProperties 상태:");
        foreach (var kv in PhotonNetwork.LocalPlayer.CustomProperties)
        {
            Debug.Log($"  Key: {kv.Key}, Value: {kv.Value}");
        }

        if (PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue("team", out object teamVal))
        {
            Debug.Log($"[GameManager] LocalPlayer 팀 정보: {teamVal}");
        }
        else
        {
            Debug.LogWarning("[GameManager] LocalPlayer 팀 정보가 없음 (team 키가 없음)");
        }

        Debug.Log("=== 전체 플레이어 팀 정보 확인 ===");
        foreach (var player in PhotonNetwork.PlayerList)
        {
            string nick = player.NickName;
            string team = player.CustomProperties.TryGetValue("team", out object t) ? t.ToString() : "없음";
            bool ready = player.CustomProperties.TryGetValue("Ready", out object r) && (bool)r;

            Debug.Log($"닉네임: {nick}, 팀: {team}, Ready: {ready}, ActorNumber: {player.ActorNumber}");
        }

        Debug.Log("=== [GameManager] Start 끝 ===");
        StartCoroutine(SpawnPlayerWithDelay());
        GameStart();
    }

    [PunRPC]
    private void SetStartTime(double start)
    {
        startTime = start;
        StartCoroutine(GameTimer());
    }
 

    private IEnumerator GameTimer()
    {
        while (true)
        {
            double elapsed = PhotonNetwork.Time - startTime;
            float remaining = matchDuration - (float)elapsed;

            if (remaining <= 0)
                break;

            UpdateTimerUI(remaining);
            yield return null;
        }

        GameEnd();
    }

    private void UpdateTimerUI(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60);
        int seconds = Mathf.FloorToInt(time % 60);
        timerText.text = $"{minutes:00}:{seconds:00}";
    }

    private IEnumerator SpawnPlayerWithDelay()
    {
        // team 프로퍼티가 들어올 때까지 대기
        yield return new WaitUntil(() => PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey("team"));

        string team = PhotonNetwork.LocalPlayer.CustomProperties["team"].ToString();
        int actorIndex = PhotonNetwork.LocalPlayer.ActorNumber; // 고유 번호로 spawn 위치를 결정

        Transform[] spawnArray = team == "Team1" ? team1SpawnPoints : team2SpawnPoints;

        if (spawnArray.Length == 0)
        {
            Debug.LogError("스폰 포인트 배열이 비어 있음!");
            yield break;
        }

        // 겹치지 않도록 mod 연산 사용
        Transform spawnPoint = spawnArray[actorIndex % spawnArray.Length];

        Debug.Log($"[{team}] 팀 위치에 플레이어 스폰 at {spawnPoint.name}");

        PhotonNetwork.Instantiate(playerPrefabName, spawnPoint.position, spawnPoint.rotation);
    }

    public void RegisterPlayer(Collider col, BaseController playerController)
    {
        playerDic[col] = playerController;
        //호출되면 그 플레이어의 콜라이더를 딕셔너리에 추가
    }

    public BaseController GetPlayer(Collider col)
    {
        playerDic.TryGetValue(col, out BaseController playerController);
        //키를 넣으면 playerController를 반환
        return playerController;

    }

    public void GameStart()
    {
        Debug.Log("게임 스타트");
        if (PhotonNetwork.IsMasterClient)
        {
            double start = PhotonNetwork.Time;
            photonView.RPC("SetStartTime", RpcTarget.All, start);
        }
    }

    public void GameEnd()
    {
        Debug.Log("게임 엔드");

        // 현재 유저가 속한 팀 가져오기
        string myTeam = PhotonNetwork.LocalPlayer.CustomProperties["team"].ToString();

        //승리 팀 판단
        string winningTeam = GridManager.Instance.GetWinningTeam();
        Debug.Log($"내 팀: {myTeam}, 승리 팀: {winningTeam}");

        //팀이 이긴 경우
        bool isWin = (winningTeam != "Draw") && (myTeam == winningTeam);

        FirebaseManager.UploadMatchResult(isWin);

        MatchData.LastRoomName = PhotonNetwork.CurrentRoom.Name;

        PhotonNetwork.LeaveRoom();

        SceneManager.LoadScene("LoginScene"); //씬 이름 변경 예정
    }
}
