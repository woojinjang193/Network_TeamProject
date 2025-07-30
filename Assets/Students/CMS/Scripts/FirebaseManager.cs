using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Extensions;
using Firebase;
using Firebase.Auth;
using Firebase.Database;

public class FirebaseManager : MonoBehaviour
{
    private static FirebaseManager instance;
    public static FirebaseManager Instance => instance;

    private static FirebaseApp app;
    public static FirebaseApp App => app;

    private static FirebaseAuth auth;
    public static FirebaseAuth Auth => auth;

    private static DatabaseReference dbRef;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
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
        FirebaseUser user = Auth.CurrentUser;
        if (user == null)
        {
            Debug.LogWarning("Firebase 로그인 안 되어 있음");
            return;
        }

        string uid = user.UserId;
        DatabaseReference userRef = dbRef.Child("users").Child(uid);  // 사용자 데이터 경로 지정

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
                    if (snapshot.Child("wins").Exists) wins = int.Parse(snapshot.Child("wins").Value.ToString());
                    if (snapshot.Child("losses").Exists) losses = int.Parse(snapshot.Child("losses").Value.ToString());
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
        FirebaseUser user = Auth.CurrentUser;
        if (user == null)
        {
            Debug.LogWarning("Firebase 로그인 안 되어 있음");
            return;
        }

        string uid = user.UserId;
        DatabaseReference userRef = dbRef.Child("users").Child(uid);

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
}
