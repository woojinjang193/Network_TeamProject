using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManagerCopy : Singleton<GameManagerCopy>
{
    public bool IsGameEnd { get; private set; }

    private Dictionary<Collider, BaseController> playerDic = new();

    [Header("팀별 스폰 위치")]
    [SerializeField] public Transform[] team1SpawnPoints;   // Team1 스폰 
    [SerializeField] public Transform[] team2SpawnPoints;   // Team2 스폰 

    private double startTime;
    [SerializeField] private float matchDuration = 180f;
    [SerializeField] private TMP_Text timerText;
    private PhotonView photonView;

    protected override void Awake()
    {
        base.Awake();
        photonView = GetComponent<PhotonView>();
    }

    private void Start()
    {
        Debug.Log("=== [GameManager] Start ===");
        StartCoroutine(InitAfterPhotonReady());
    }

    private IEnumerator InitAfterPhotonReady()
    {
        while (!PhotonNetwork.IsConnectedAndReady || PhotonNetwork.LocalPlayer == null || !PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey("team"))
        {
            yield return null;
        }

        Debug.Log("=== 전체 플레이어 팀 정보 확인 ===");
        foreach (var player in PhotonNetwork.PlayerList)
        {
            string nick = player.NickName;
            string team = player.CustomProperties.TryGetValue("team", out object t) ? t.ToString() : "없음";
            bool ready = player.CustomProperties.TryGetValue("Ready", out object r) && (bool)r;
            Debug.Log($"닉네임: {nick}, 팀: {team}, Ready: {ready}, ActorNumber: {player.ActorNumber}");
        }

        yield return StartCoroutine(SpawnPlayerWithDelay());

        if (PhotonNetwork.IsMasterClient)
        {
            StartCoroutine(WaitForRoomManagerAndSpawnBots());
        }

        if (PhotonNetwork.IsMasterClient)
        {
            GameStart();  // 마스터만 호출
        }
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
            if (remaining <= 0) break;
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

        // 팀별 프리팹 이름 분기
        string prefabName = team switch
        {
            "Team1" => "Player_Purple",
            "Team2" => "Player_Yellow",
            _ => null
        };

        if (string.IsNullOrEmpty(prefabName))
        {
            Debug.LogError($"[SpawnPlayerWithDelay] 알 수 없는 팀: {team}");
            yield break;
        }

        PhotonNetwork.Instantiate(prefabName, spawnPoint.position, spawnPoint.rotation);
        Debug.Log($"플레이어 생성 완료: {prefabName}, 위치: {spawnPoint.position}");
    }
    private void SpawnBots()
    {
        var roomManager = Manager.Net.roomManager;
        if (roomManager == null)
        {
            Debug.LogError("SpawnBots: RoomManager 찾을 수 없음");
            return;
        }

        var bots = roomManager.GetBots();
        Debug.Log($"[SpawnBots] 봇 수: {bots.Count}");

        for (int i = 0; i < bots.Count; i++)
        {
            var bot = bots[i];

            Transform[] spawnArray = bot.Team == "Team1" ? team1SpawnPoints : team2SpawnPoints;
            if (spawnArray.Length == 0)
            {
                Debug.LogError("SpawnBots: 스폰 포인트 없음");
                continue;
            }

            Transform spawnPoint = spawnArray[i % spawnArray.Length];

            //팀별 봇 프리팹 이름 결정
            string prefabName = bot.Team switch
            {
                "Team1" => "AI_Purple",
                "Team2" => "AI_Yellow",
                _ => null
            };

            if (string.IsNullOrEmpty(prefabName))
            {
                Debug.LogError($"SpawnBots: 알 수 없는 팀 {bot.Team}");
                continue;
            }

            GameObject botGO = PhotonNetwork.Instantiate(prefabName, spawnPoint.position, spawnPoint.rotation);

            var ai = botGO.GetComponent<AIController>();
            if (ai != null)
            {
                ai.MyTeam = bot.Team == "Team1" ? Team.Team1 : Team.Team2;
                Debug.Log($"봇 생성 완료: {bot.Name}, 프리팹: {prefabName}, 팀: {ai.MyTeam}");
            }
        }
    }
    private IEnumerator WaitForRoomManagerAndSpawnBots()
    {
        while (Manager.Net.roomManager == null)
        {
            Debug.Log("[WaitForRoomManagerAndSpawnBots] RoomManager를 기다리는 중...");
            yield return null;
        }

        Debug.Log("[WaitForRoomManagerAndSpawnBots] RoomManager 찾음 → SpawnBots 실행");
        SpawnBots();
    }

    public void RegisterPlayer(Collider col, BaseController playerController)
    {
        playerDic[col] = playerController;
        //호출되면 그 플레이어의 콜라이더를 딕셔너리에 추가
    }

    public BaseController GetPlayer(Collider col)
    {
        playerDic.TryGetValue(col, out BaseController playerController); //키를 넣으면 playerController를 반환
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

    private void GameEnd()
    {
        Debug.Log("게임 엔드");

        //플레이어 오브젝트 수동 제거
        //DestroyPlayers();

        // foreach (var playerObj in GameObject.FindGameObjectsWithTag("Player"))
        // {
        //     var pv = playerObj.GetComponent<PhotonView>();
        //     if (pv != null && pv.IsMine)
        //     {
        //         PhotonNetwork.Destroy(playerObj);
        //         Debug.Log($"플레이어 오브젝트 제거됨: {playerObj.name}");
        //     }
        // }

        // 현재 유저가 속한 팀 가져오기
        string myTeam = PhotonNetwork.LocalPlayer.CustomProperties["team"].ToString();

        //승리 팀 판단
        string winningTeam = Manager.Grid.GetWinningTeam();
        Debug.Log($"내 팀: {myTeam}, 승리 팀: {winningTeam}");

        //팀이 이긴 경우
        bool isWin = (winningTeam != "Draw") && (myTeam == winningTeam);

        FirebaseManager.UploadMatchResult(isWin);

        ChangeToLoginScene();
    }

    private void ChangeToLoginScene()
    {
        AsyncOperation async = SceneManager.LoadSceneAsync("LoginScene");
        if (async != null)
            async.completed += (AsyncOperation op) =>
            {
                Debug.Log("로그인 씬 불러오기 완료");
                Manager.Net.roomManager = FindObjectOfType<RoomManager>();
                Manager.Net.roomManager.RoomReInit();

            };
    }
    private void ChangeToGameScene()
    {
        AsyncOperation async = SceneManager.LoadSceneAsync("JWJ_SampleScene");
        if (async != null)
        {
            async.completed += (AsyncOperation op) =>
            {
                Debug.Log("게임 씬 로딩 완료");

                SpawnBots();
                Manager.Net.roomManager?.RoomReInit();
            };
        }
    }
    private void DestroyPlayers()
    {
        foreach (var playerObj in playerDic.Values)
        {
            PhotonNetwork.Destroy(playerObj.gameObject);
        }
    }
}
