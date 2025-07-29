using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : Singleton<GameManager>
{
    public bool IsGameEnd {  get; private set; }

    private Dictionary<Collider, PlayerTestController> playerDic = new();
    //플레이어의 콜라이더와 컨트롤러를 넣을 딕셔너리 

    [Header("팀별 스폰 위치")]
    [SerializeField] private Transform[] team1SpawnPoints;   // Team1 스폰 
    [SerializeField] private Transform[] team2SpawnPoints;   // Team2 스폰 
    [SerializeField] private string playerPrefabName = "Player";

    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(gameObject);
    }
    private void Start()
    {
        Debug.Log("게임매니저 스타트");
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

    public void RegisterPlayer(Collider col, PlayerTestController playerController)
    {
        playerDic[col] = playerController;
        //호출되면 그 플레이어의 콜라이더를 딕셔너리에 추가
    }

    public PlayerTestController GetPlayer(Collider col)
    {
        playerDic.TryGetValue(col, out PlayerTestController playerTestController);
        //키를 넣으면 playerTestController를 반환
        return playerTestController;

    }

    public void GameStart()
    {
        Debug.Log("게임 스타트");
    }

    public void GameEnd()
    {
        Debug.Log("게임 엔드");

        // 현재 유저가 속한 팀 가져오기
        //string myTeam = PhotonNetwork.LocalPlayer.CustomProperties["team"].ToString();

        // 승리 팀 판단
        //string winningTeam = GridManager.GetInstance().GetWinningTeam();
        //Debug.Log($"내 팀: {myTeam}, 승리 팀: {winningTeam}");

        //팀이 이긴 경우
       // bool isWin = (winningTeam != "Draw") && (myTeam == winningTeam);

       // FirebaseManager.UploadMatchResult(isWin);

        SceneManager.LoadScene("LoginScene"); //씬 이름 변경 예정
    }
}
