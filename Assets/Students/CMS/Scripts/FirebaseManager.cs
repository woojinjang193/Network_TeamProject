using System;
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
    private static FirebaseDatabase db;
    public static FirebaseDatabase DB => db;

    private static DatabaseReference dbRef;
    private static DatabaseReference leaderRef;
    private static DatabaseReference userRef;
    private static FirebaseUser user;
    private static string uid;
    
    // 리더보드
    public LeaderBoard LeaderBoard;

    protected override void Awake() { base.Awake(); }

    public void Start()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task => {
            var status = task.Result;
            if (status == DependencyStatus.Available)
            {
                app = FirebaseApp.DefaultInstance;
                auth = FirebaseAuth.DefaultInstance;
                db= FirebaseDatabase.DefaultInstance;
                dbRef = FirebaseDatabase.DefaultInstance.RootReference;
                db.GoOnline(); // 데이터베이스를 온라인을 시켜줘야 함
                
                leaderRef = dbRef.Child("LeaderBoard");
                LeaderBoard = FindObjectOfType<LeaderBoard>(true);
                
                Debug.Log("Firebase 설정 완료");
            }
            else
            {
                Debug.LogError("Firebase 초기화 실패: " + status);
            }
        });

    }

    public static void UploadMatchResult(bool isWin) // 매치 결과 승리, 패배 업로드
    {
        user = Auth.CurrentUser;
        uid = user.UserId;
        userRef = dbRef.Child("users").Child(uid);  // 사용자 데이터 경로 지정
        
        if (user == null)
        {
            Debug.LogWarning("Firebase 로그인 안 되어 있음");
            return;
        }

        // 기존 데이터 불러오기
        userRef.GetValueAsync().ContinueWithOnMainThread(task => {
            if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;

                string userName = "Unknown";
                int wins = 0;
                int losses = 0;

                // 기존 데이터가 있으면 값 읽어오기
                if (snapshot.Exists)
                {
                    if (snapshot.Child("userName").Exists)
                    {
                        userName = snapshot.Child("userName").Value.ToString();
                    }
                    if (snapshot.Child("wins").Exists)
                    {
                        wins = Convert.ToInt32(snapshot.Child("wins").Value);
                    }

                    if (snapshot.Child("losses").Exists)
                    {
                        losses = Convert.ToInt32(snapshot.Child("losses").Value);
                    }
                }

                // 결과 반영
                if (isWin) wins++;
                else losses++;

                // 승률 계산
                float winRate = (wins + losses) > 0 ? (float)wins / (wins + losses) : 0f;

                // Firebase에 업데이트할 데이터 구성
                UserData data = new(userName, wins, losses, winRate);
                string json = JsonUtility.ToJson(data);
                
                // 데이터베이스에 반영
                userRef.SetRawJsonValueAsync(json).ContinueWithOnMainThread(updateTask => {
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
    
    public static void UploadNickname(string nickname) // 닉네임 업로드 함수
    { 
        user = Auth.CurrentUser;
        uid = user.UserId;
        userRef = dbRef.Child("users").Child(uid);  // 사용자 데이터 경로 지정
        
        if (user == null)
        {
            Debug.LogWarning("Firebase 로그인 안 되어 있음");
            return;
        }

        UserData userData = new(nickname, 0, 0, 0f); //들어온 이름을 기준으로 유저데이터 객체 생성
        string json = JsonUtility.ToJson(userData); //Json으로 변환


        userRef.SetRawJsonValueAsync(json).ContinueWithOnMainThread(task => { // UID 경로 밑에 저장
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
    
    
    
    public void GetLeaderBoard() // 리더보드에 옮기기 위해 데이터베이스에서 값을 가져오는 함수
    {
        
        List<UserData> userDatas =  new List<UserData>(); // 리스트 형태로 유저데이터들을 넘긴다
        dbRef.Child("users").OrderByChild("wins").LimitToLast(10).GetValueAsync().ContinueWithOnMainThread(task =>
        { //승리 수를 기준으로 정렬해서 10개까지만 가져옴
            if (task.IsCanceled)
            {
                Debug.LogWarning("리더보드 불러오기 취소");
            }

            if (task.IsFaulted)
            {
                Debug.LogWarning($"리더보드 불러오기 실패{task.Exception?.InnerException?.Message}");
            }

            DataSnapshot snapshot = task.Result; // 스냅샷에 넣음
            foreach (var entry in snapshot.Children) 
            {
                string json = entry.GetRawJsonValue();
                UserData fromJson = JsonUtility.FromJson<UserData>(json);
                userDatas.Add(fromJson);
            }

            LeaderBoard.UserDataToLeaderBoard(userDatas);
        });
    }
}
