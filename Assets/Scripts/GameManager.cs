using ExitGames.Client.Photon.StructWrapping;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using Debug = UnityEngine.Debug;

public class GameManager : MonoBehaviour
{
    public bool IsGameEnd { get; set; }

    [SerializeField] private float waitForOtherPlayersTime = 60f;

    private Dictionary<Collider, BaseController> playerDic = new();

    [Header("팀별 스폰 위치")]
    [SerializeField] public Transform[] team1SpawnPoints;   // Team1 스폰 
    [SerializeField] public Transform[] team2SpawnPoints;   // Team2 스폰 

    //게임 스타트 UI
    [SerializeField] private GameStartUI gameStartUI;
    //게임결과 UI
    [SerializeField] private GameResultUI gameResultUI;
    [SerializeField] private GameObject inkGauge;

    private double startTime;
    [SerializeField] private float matchDuration = 180f;
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private GameObject timerPanel;

    private PhotonView photonView;
    private int spawnedCharCount = 0; //
    private Animator timerAnimation;

    public static event Action OnGameStarted;
    public static event Action OnGameEnded;

    private Coroutine waitStartCoroutine;

    private int botCount = 0;
    private void Awake()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            BotNameList.Reset(); //봇이름 리스트 초기화
        }

        Manager.Game = this;
        photonView = GetComponent<PhotonView>();
        timerAnimation = GetComponentInChildren<Animator>();
    }

    private void Start()
    {
        Debug.Log("=== [GameManager] Start ===");

        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("bots", out object botsRaw)) //봇 카운트
        {
            foreach (var obj in (object[])botsRaw)
            {
                botCount++;
            }
        }

        StartCoroutine(InitAfterPhotonReady());
    }

    private IEnumerator InitAfterPhotonReady()
    {
        if (!SceneManager.GetActiveScene().name.StartsWith("Map"))
        {
            Debug.LogWarning("게임 맵이 아님. 플레이어 생성 로직 생략");
            yield break;
        }
        playerDic.Clear(); // 딕셔너리 초기화  

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

        if (time < 11)
        {
            timerText.color = Color.red;
            timerAnimation.SetTrigger("signal");
        }
    }

    private IEnumerator SpawnPlayerWithDelay()
    {
        if (!SceneManager.GetActiveScene().name.StartsWith("Map"))
        {
            Debug.LogWarning("게임 맵이 아님. 플레이어 스폰 생략");
            yield break;
        }
        yield return new WaitUntil(() => PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey("team"));

        string myTeam = PhotonNetwork.LocalPlayer.CustomProperties["team"].ToString();
        Transform[] spawnArray = myTeam == "Team1" ? team1SpawnPoints : team2SpawnPoints;

        if (spawnArray.Length == 0)
        {
            Debug.LogError("스폰 포인트 배열이 비어 있음!");
            yield break;
        }

        int myActorNumber = PhotonNetwork.LocalPlayer.ActorNumber;
        int myIndex = GetTeamIndex(myTeam, myActorNumber);

        if (myIndex >= spawnArray.Length)
        {
            Debug.LogWarning("스폰 포인트 부족, 0번으로 fallback");
            myIndex = 0;
        }

        Transform spawnPoint = spawnArray[myIndex];

        string prefabName = myTeam switch
        {
            "Team1" => "Player_Purple",
            "Team2" => "Player_Yellow",
            _ => null
        };

        PhotonNetwork.Instantiate(prefabName, spawnPoint.position, spawnPoint.rotation);
    }

    [PunRPC]
    void NotifyCharSpawned() /// 플레이 준비가 완료되면 호출
    {
        spawnedCharCount++;

        int totalChars = PhotonNetwork.PlayerList.Length + botCount;

        Debug.Log($"참가완료 캐릭터 수: {spawnedCharCount}/{totalChars}");

        if (spawnedCharCount == totalChars)
        {
            if (waitStartCoroutine != null)
            {
                StopCoroutine(waitStartCoroutine);
                waitStartCoroutine = null;
            }
            photonView.RPC("OpenGameStartUITogether", RpcTarget.All);
            photonView.RPC("SwitchBGMTogether", RpcTarget.All);
            return;
        }

        if (waitStartCoroutine == null && spawnedCharCount > 0) //참가자가 1명 이상일때 한번 실행
        {
            waitStartCoroutine = StartCoroutine(WaitAndStartGame());
        }
    }

    [PunRPC]
    private void OpenGameStartUITogether() //NotifyCharSpawned에서 수행
    {
        gameStartUI.openGameStartUI();
    }

    [PunRPC]
    private void SwitchBGMTogether() //NotifyCharSpawned에서 수행
    {
        // 스테이지에 따라 음악 다르게 재생
        string sceneName = SceneManager.GetActiveScene().name;
        if (sceneName == "Map1")
        {
            Manager.Audio.SwitchBGM("Stage1");
        }
        else
        {
            Manager.Audio.SwitchBGM("Stage2");
        }
    }

    private IEnumerator WaitAndStartGame() //참가자가 2분동안 꽉차지않으면 게임 그냥 실행
    {
        Debug.Log($"다른 플레이어 기다리는중..{waitForOtherPlayersTime}초 대기 타이머 시작");
        yield return new WaitForSeconds(waitForOtherPlayersTime);
        photonView.RPC("OpenGameStartUITogether", RpcTarget.All);
        waitStartCoroutine = null;
    }

    private void SpawnBots()
    {
        if (!PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("bots", out object botsRaw))
            return;

        object[] bots = botsRaw as object[];
        if (bots == null || bots.Length == 0) return;

        foreach (var botObj in bots)
        {
            var bot = (ExitGames.Client.Photon.Hashtable)botObj;
            string team = (string)bot["team"];
            string name = (string)bot["name"];

            Transform[] spawnArray = team == "Team1" ? team1SpawnPoints : team2SpawnPoints;
            if (spawnArray.Length == 0) continue;

            int fakeActorNumber = GetBotFakeActorNumber(name);
            int index = GetTeamIndex(team, fakeActorNumber);
            if (index >= spawnArray.Length) index = 0;

            Transform spawnPoint = spawnArray[index];

            string prefabName = team switch
            {
                "Team1" => "AI_Purple",
                "Team2" => "AI_Yellow",
                _ => null
            };

            GameObject botGO = PhotonNetwork.InstantiateRoomObject(prefabName, spawnPoint.position, spawnPoint.rotation);

            var ai = botGO.GetComponent<AIController>();
            if (ai != null)
            {
                ai.MyTeam = team == "Team1" ? Team.Team1 : Team.Team2;
                Debug.Log($"봇 생성 완료: {name}, 위치: {spawnPoint.position}");
            }
        }
    }


    private IEnumerator WaitForRoomManagerAndSpawnBots()
    {
        //while (Manager.Net.roomManager == null)
        //{
        //    Debug.Log("[WaitForRoomManagerAndSpawnBots] RoomManager를 기다리는 중...");
        //    yield return null;
        //}

        Manager.Net.roomManager = FindObjectOfType<RoomManager>();
        if (Manager.Net.roomManager == null)
        {
            Debug.Log("[WaitForRoomManagerAndSpawnBots] RoomManager를 기다리는 중...");
            yield return null;
        }

        Debug.Log("[WaitForRoomManagerAndSpawnBots] RoomManager 찾음, SpawnBots 실행");
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
        IsGameEnd = false;
        if (PhotonNetwork.IsMasterClient)
        {
            double start = PhotonNetwork.Time;
            photonView.RPC("SetStartTime", RpcTarget.All, start);
            photonView.RPC("RpcGameStart", RpcTarget.All);
        }

    }

    [PunRPC]
    void RpcGameStart()
    {
        OnGameStarted?.Invoke();
    }

    [PunRPC]
    void RpcGameEnd()
    {
        OnGameEnded?.Invoke(); // 캐릭터들 정지
        OnGameEnded = null;
    }

    private void GameEnd()
    {
        IsGameEnd = true;
        // 게임 종료 사운드 재생 및 브금 종료
        Manager.Audio.PlayEffect("GameSet");
        Manager.Audio.StopAllSounds();

        Debug.Log("게임 엔드");
        photonView.RPC("RpcGameEnd", RpcTarget.All);

        // 현재 유저가 속한 팀 가져오기
        string myTeam = PhotonNetwork.LocalPlayer.CustomProperties["team"].ToString();

        //승리 팀 판단
        string winningTeam = Manager.Grid.GetWinningTeam();



        Debug.Log($"내 팀: {myTeam}, 승리 팀: {winningTeam}");

        //팀이 이긴 경우
        bool isWin = false;
        switch (winningTeam)
        {
            case "Purple":
                if (myTeam == "Team1")
                {
                    isWin = true;
                    Debug.Log($"우리팀이 승리함 {myTeam} {winningTeam}");
                }
                break;
            case "Yellow":
                if (myTeam == "Team2")
                {
                    isWin = true;
                    Debug.Log($"우리팀이 승리함 {myTeam} {winningTeam}");
                }
                break;
            default:
                Debug.Log($"승리팀 입력에 제대로 되지 않음 승리 팀 : {winningTeam} 내 팀: {myTeam} ");
                break;
        }

        

        FirebaseManager.UploadMatchResult(isWin);

        float team1Rate = Manager.Grid.Team1Rate;
        float team2Rate = Manager.Grid.Team2Rate;

        //PlayerOff(); //플레이어 비활성화

        timerPanel.gameObject.SetActive(false);

        inkGauge.SetActive(false);
        if (PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("ShowResultUI", RpcTarget.All, winningTeam, team1Rate, team2Rate);
        }
    }

    [PunRPC]
    void ShowResultUI(string winner, float team1Rate, float team2Rate)
    {
        gameResultUI.UIOpen(winner, team1Rate / 100f, team2Rate / 100f);
    }

    public void ChangeToLoginScene()
    {
        PhotonNetwork.DestroyPlayerObjects(PhotonNetwork.LocalPlayer);

        AsyncOperation async = SceneManager.LoadSceneAsync("LoginScene");
        if (async != null)
            async.completed += (AsyncOperation op) =>
            {
                Debug.Log("로그인 씬 불러오기 완료");
                Manager.Audio.SwitchBGM("defaultBGM");
                Manager.Audio.SwitchAmbient("defaultAmbient");
                Manager.Net.roomManager = FindObjectOfType<RoomManager>();
                Manager.Net.roomManager.RoomReInit();

                if (PhotonNetwork.IsMasterClient)
                {
                    PhotonNetwork.CurrentRoom.IsOpen = true; // 다시 유저 받기 가능하게
                    PhotonNetwork.CurrentRoom.IsVisible = true;  // 로비에 표시
                }
            };
    }
    private void DestroyPlayers()
    {
        foreach (var playerObj in playerDic.Values)
        {
            PhotonNetwork.Destroy(playerObj.gameObject);
        }
    }

    public void PlayerOff()
    {
        foreach (BaseController player in playerDic.Values)
        {
            if (player != null && player.gameObject != null)
            {
                player.gameObject.SetActive(false);
            }
        }
    }
    private int GetTeamIndex(string team, int myActorNumber)
    {
        List<int> allActorNumbers = new();

        // 플레이어 포함
        foreach (var player in PhotonNetwork.PlayerList)
        {
            if (player.CustomProperties.TryGetValue("team", out object t) && (string)t == team)
                allActorNumbers.Add(player.ActorNumber);
        }

        // 봇 포함
        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("bots", out object botsRaw))
        {
            foreach (var botObj in (object[])botsRaw)
            {
                var bot = (ExitGames.Client.Photon.Hashtable)botObj;
                if ((string)bot["team"] == team)
                    allActorNumbers.Add(GetBotFakeActorNumber(bot["name"].ToString()));
            }
        }

        allActorNumbers.Sort(); // 항상 같은 순서
        return allActorNumbers.IndexOf(myActorNumber);
    }

    // 봇 이름을 기반으로 고유 인덱스를 생성 (예: [BOT] Alpha1 → 9001)
    private int GetBotFakeActorNumber(string botName)
    {
        string digits = System.Text.RegularExpressions.Regex.Match(botName, @"\d+").Value;
        return 1000 + int.Parse(digits);
    }
}
