using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Firebase.Auth;
using Firebase.Extensions;

public class NicknamePanel : MonoBehaviour
{
    [SerializeField] GameObject lobbyPanel;
    [SerializeField] GameObject loginPanel;
    [SerializeField] TMP_InputField nicknameInput;

    [SerializeField] Button confirmButton;
    [SerializeField] Button backButton;

    private void Awake()
    {
        confirmButton.onClick.AddListener(Confirm);
        backButton.onClick.AddListener(Back);
    }

    private void Confirm()
    {
        UserProfile profile = new UserProfile();
        profile.DisplayName = nicknameInput.text;


        FirebaseUser user = FirebaseManager.Auth.CurrentUser;
        user.UpdateUserProfileAsync(profile)
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsCanceled)
                {
                    Debug.LogError("닉네임 업데이트 작업이 취소되었습니다.");
                    return;
                }
                if (task.IsFaulted)
                {
                    Debug.LogError($"닉네임 업데이트 중 오류 발생: {task.Exception}");
                    return;
                }
                Debug.Log("닉네임 업데이트 성공!");
                FirebaseManager.UploadNickname(nicknameInput.text);
                lobbyPanel.SetActive(true);
                gameObject.SetActive(false);
            });
    }

    private void Back()
    {
        FirebaseManager.Auth.SignOut();
        loginPanel.SetActive(true);
        gameObject.SetActive(false);
        
    }
}
