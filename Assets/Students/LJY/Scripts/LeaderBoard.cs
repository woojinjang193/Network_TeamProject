using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.ShaderGraph;
using UnityEngine;

public class LeaderBoard : MonoBehaviour
{
    // 데이터 베이스에서 가져오는 유저 데이터를 담을 리스트
    public List<LeaderBoardEntry> ranker = new List<LeaderBoardEntry>();
    [SerializeField] public List<UserData> userData = new List<UserData>();
    private Dictionary<int,RankItem> rankDict = new Dictionary<int,RankItem>();

    // 타이머 세팅
    [Header("Set Timer")]
    [SerializeField] private float updateTime = 10;
    // UI 세팅
    [Header("Set UI References")]
    [SerializeField] private Transform scrollViewContent;
    [SerializeField] private GameObject numberOne;
    [SerializeField] private GameObject numberTwo;
    [SerializeField] private GameObject numberThree;
    [SerializeField] private GameObject others;

    // UI 최신화 코루틴
    private Coroutine UpdateUIRoutine;
    private WaitForSecondsRealtime updateReturn;
    private void OnEnable()
    {
        if (Manager.FB != null)
        {
            if(Manager.FB.LeaderBoard == null)
            {
                Manager.FB.LeaderBoard = this;
            }
            Manager.FB.GetLeaderBoard(); // 켜지는 순간 리더보드 갱신
        }
        else
        {
            StartCoroutine(WaitFireManager());
        }

        updateReturn = new WaitForSecondsRealtime(updateTime); // UI 업데이트 기다리는 시간 설정
        UpdateUIRoutine = StartCoroutine(StartUIUpdate()); // 지정된 주기마다 업데이트
    }

    private void OnDisable()
    {
        if (UpdateUIRoutine != null)
        {
            StopCoroutine(UpdateUIRoutine);
        }
        UpdateUIRoutine = null;
    }

    private IEnumerator StartUIUpdate()
    {
        yield return updateReturn;
        Manager.FB.GetLeaderBoard();
    }

    private IEnumerator WaitFireManager()
    {
        while (Manager.FB == null)
        {
            Debug.Log("파이어베이스 기다리는중...");
            yield return null;
        }
        Manager.FB.GetLeaderBoard(); // 켜지는 순간 리더보드 갱신
    }

    public void UserDataToLeaderBoard(List<UserData> userData) // 유저 데이터 리스트를 가지고 UI를 최신화 한다. 파이어베이스의 GetLeaderBoard에서 호출된다.
    {
        Debug.Log("가져온 데이터로 리더보드 리스트 업");
        ranker.Clear();
        this.userData = userData;
        userData.Sort((a, b) => b.wins.CompareTo(a.wins));
        for (int i = 0; i < 10; i++)
        {
            ListUp(userData[i].userName,userData[i].wins,userData[i].winRate);
        }

        SetLeaderBoard();
    }

    private void SetLeaderBoard() // 리더보드 UI를 세팅하는 함수. UserDataToLeaderBoard로 호출된다.
    {
        Debug.Log("만들어진 리스트로 리더보드 만들기");

        int index = 0;
        foreach (var key in rankDict.Keys)
        {
            Destroy(rankDict[key].gameObject);
        }
        rankDict.Clear();
        
        //  1등 설정
        RankItem one = Instantiate(numberOne,scrollViewContent).GetOrAddComponent<RankItem>();
        rankDict[index] = one;
        index++;
        one.Init();
        one.SetRankerInfo(ranker[0].nickName,ranker[0].win,ranker[0].winRate,1);
        // 2등 설정
        RankItem two = Instantiate(numberTwo,scrollViewContent).GetOrAddComponent<RankItem>();
        rankDict[index] = two;
        index++;
        two.Init();
        two.SetRankerInfo(ranker[1].nickName,ranker[1].win,ranker[1].winRate,2);
        // 3등 설정
        RankItem three = Instantiate(numberThree,scrollViewContent).GetOrAddComponent<RankItem>();
        rankDict[index] = three;
        index++;
        three.Init();
        three.SetRankerInfo(ranker[2].nickName,ranker[2].win,ranker[2].winRate,3);
        // 나머지 설정
        for (int i = 3; i < ranker.Count; i++)
        {
            RankItem other = Instantiate(others,scrollViewContent).GetOrAddComponent<RankItem>();
            rankDict[i] = other;
            other.Init();
            other.SetRankerInfo(ranker[i].nickName,ranker[i].win,ranker[i].winRate,i+1);
        }
    }
    
    private void ListUp(string nickName, int win, float winRate) // 리스트에 유저데이터 추가
    {
        LeaderBoardEntry entry = new LeaderBoardEntry(nickName,win,winRate);
        
        ranker.Add(entry);
    }
}

[System.Serializable]
public class LeaderBoardEntry
{
    public string nickName;
    public int win;
    public float winRate;

    public LeaderBoardEntry(string nickName, int win, float winRate)
    {
        this.nickName = nickName;
        this.win = win;
        this.winRate = winRate;
    }
}

