using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Firebase.Extensions;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using WebSocketSharp;

public class FirebaseManager : Singleton<FirebaseManager>
{
    private static FirebaseApp app;
    public static FirebaseApp App => app;

    private static FirebaseAuth auth;
    public static FirebaseAuth Auth => auth;

    private static DatabaseReference dbRef;
    private static DatabaseReference leaderRef;
    private static DatabaseReference userRef;
    private static FirebaseUser user;
    private static string uid;
    
    // 리더보드
    public static LeaderBoard LeaderBoard;

    protected override void Awake()
    {
        base.Awake();
    }

    public void Start()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task => {
            var status = task.Result;
            if (status == DependencyStatus.Available)
            {
                app = FirebaseApp.DefaultInstance;
                auth = FirebaseAuth.DefaultInstance;
                dbRef = FirebaseDatabase.DefaultInstance.RootReference;
                userRef = dbRef.Child("users").Child(uid);  // 사용자 데이터 경로 지정
                user = Auth.CurrentUser;
                uid = user.UserId;
                LeaderBoard = FindObjectOfType<LeaderBoard>();
                dbRef.Child("users").ChildAdded += GetLeaderboard;
                Debug.Log("Firebase 설정 완료");
            }
            else
            {
                Debug.LogError("Firebase 초기화 실패: " + status);
            }
        });

    }

    public static void UploadMatchResult(bool isWin) // 매치 결과 업로드
    {
        if (user == null)
        {
            Debug.LogWarning("Firebase 로그인 안 되어 있음");
            return;
        }

        // 기존 데이터 조회
        userRef.GetValueAsync().ContinueWithOnMainThread(task => {
            if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;

                int wins = 0;
                int losses = 0;

                // 기존 데이터가 있으면 값 읽어오기
                if (snapshot.Exists)
                {
                    if (snapshot.Child("wins").Exists)
                    {
                        wins = (int)(long)snapshot.Child("wins").Value;
                    }

                    if (snapshot.Child("losses").Exists)
                    {
                        losses = (int)(long)snapshot.Child("losses").Value;
                    }
                }

                // 결과 반영
                if (isWin) wins++;
                else losses++;

                // 승률 계산
                float winRate = (wins + losses) > 0 ? (float)wins / (wins + losses) : 0f;

                // Firebase에 업데이트할 데이터 구성
                Dictionary<string, object> updates = new Dictionary<string, object>
                {
                    { "wins", wins },
                    { "losses", losses },
                    { "winRate", winRate }
                };

                // 데이터베이스에 반영
                userRef.UpdateChildrenAsync(updates).ContinueWithOnMainThread(updateTask => {
                    if (updateTask.IsCompleted)
                    {
                        Debug.Log("랭킹/승률 업로드 성공");
                    }
                    else
                    {
                        Debug.LogError("업로드 실패: " + updateTask.Exception);
                    }
                });
            }
        });
    }
    public static void UploadNickname(string nickname)
    { 
        if (user == null)
        {
            Debug.LogWarning("Firebase 로그인 안 되어 있음");
            return;
        }

        Dictionary<string, object> updates = new Dictionary<string, object>
    {
        { "nickname", nickname }
    };

        userRef.UpdateChildrenAsync(updates).ContinueWithOnMainThread(task => {
            if (task.IsCompleted)
            {
                Debug.Log("닉네임 업로드 성공");
            }
            else
            {
                Debug.LogError("닉네임 업로드 실패: " + task.Exception);
            }
        });
    }

    private void SetLeaderBoard(DataSnapshot snapshot)
    {
        // DataSnapshot snapshot = args.Snapshot;
        LeaderBoard.ranker =  new List<LeaderBoardEntry>();
        LeaderBoard.ranker.Clear();
        Debug.Log("여기는 들어옴"); //여기 들어오는것 확인 됨
        int index = 0;
        foreach (var entry in snapshot.Children)
        {
            string nickname;
            if (entry.Child("nickname").Exists)
            {
                nickname = entry.Child("nickname").Value.ToString();
            }
            else
            {
                nickname = "Unknown";
            }

            int win;
            if (entry.Child("wins").Exists)
            {
                win = (int)(long)entry.Child("wins").Value;
            }
            else
            {
                win = 0;
            }

            float winRate = 0f;
            if (entry.Child("winRate").Exists)
            {
                winRate = (float)(double)entry.Child("winRate").Value;
            }
            else
            {
                winRate = 0;
            }
            LeaderBoard.ListUp(nickname,win,winRate);
            Debug.Log($"{LeaderBoard.ranker[index].nickName}{LeaderBoard.ranker[index].win}"); // 이건 실행 안됨
            index++;
        }
    }

    private void GetLeaderboard(object sender, ChildChangedEventArgs args) => SetLeaderBoard(args.Snapshot);
}
        // dbRef.Child("users").GetValueAsync().ContinueWithOnMainThread( task =>
        // {
        //     if (task.IsCanceled)
        //     {
        //         Debug.LogWarning("값 불러오기 취소");
        //         return;
        //     }
        //
        //     if (task.IsFaulted)
        //     {
        //         string error =  task.Exception?.InnerException?.Message;
        //         
        //         Debug.LogWarning($"값 불러오기 실패{error}");
        //         return;
        //     }
        
       // });
// namespace ConsoleApp3 { internal class Program { public async Task Foo() { await Task.Delay(1000); } } }
