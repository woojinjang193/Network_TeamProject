using Photon.Pun;
using Photon.Pun.Demo.PunBasics;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TestGameManager : Singleton<GameManager>
{
    public bool IsGameEnd { get; private set; }

    private Dictionary<Collider, BaseController> playerDic = new();
    //플레이어의 콜라이더와 컨트롤러를 넣을 딕셔너리 

    [Header("팀별 스폰 위치")]
    [SerializeField] public Transform[] team1SpawnPoints;   // Team1 스폰 
    [SerializeField] public Transform[] team2SpawnPoints;   // Team2 스폰 
    [SerializeField] private string playerPrefabName = "Player_CharacterTest";

    //게임결과 UI
    [SerializeField] private GameResultUI gameResultUI;

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
        //string myTeam = PhotonNetwork.LocalPlayer.CustomProperties["team"].ToString();

        //승리 팀 판단
        string winningTeam = Manager.Grid.GetWinningTeam();
        //string winningTeam = GridManager.Instance.GetWinningTeam();
        //Debug.Log($"내 팀: {myTeam}, 승리 팀: {winningTeam}");

        //팀이 이긴 경우
        //bool isWin = (winningTeam != "Draw") && (myTeam == winningTeam);

        //FirebaseManager.UploadMatchResult(isWin);

        PlayerOff();

        //SceneManager.LoadScene("LoginScene"); //씬 이름 변경 예정

        //gameResultUI.UIOpen(winningTeam);
        if (PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("ShowResultUI", RpcTarget.All, winningTeam);
        }

    }

    [PunRPC]
    void ShowResultUI(string winner)
    {
        gameResultUI.UIOpen(winner);
    }

    private void PlayerOff()
    {
        foreach (BaseController player in playerDic.Values)
        {
            player.gameObject.SetActive(false);
        }
    }


}
