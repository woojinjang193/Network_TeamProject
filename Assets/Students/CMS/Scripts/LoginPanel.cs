using Firebase.Auth;
using Firebase.Extensions;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoginPanel : MonoBehaviour
{
    [SerializeField] GameObject signUpPanel;
    [SerializeField] GameObject lobbyPanel;
    [SerializeField] GameObject emailPanel;
    [SerializeField] GameObject nicknamePanel;

    [SerializeField] TMP_InputField idInput;
    [SerializeField] TMP_InputField passwordInput;

    [SerializeField] Button loginButton;
    [SerializeField] Button signUpButton;
    [SerializeField] Button resetPasswordButton;

    private void Awake()
    {
        signUpButton.onClick.AddListener(SignUp);
        loginButton.onClick.AddListener(Login);
        resetPasswordButton.onClick.AddListener(ResetPassword);
    }

    private void SignUp()
    {
        signUpPanel.SetActive(true);
        gameObject.SetActive(false);
    }

    private void Login()
    {
        FirebaseManager.Auth.SignInWithEmailAndPasswordAsync(idInput.text, passwordInput.text)
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsCanceled || task.IsFaulted)
                {
                    Debug.LogError($"로그인 실패: {task.Exception}");
                    return;
                }

                FirebaseUser user = task.Result.User;
                Debug.Log("로그인 성공!");

                if (!user.IsEmailVerified)
                {
                    emailPanel.SetActive(true);
                    gameObject.SetActive(false);
                    return;
                }

                if (string.IsNullOrEmpty(user.DisplayName))
                {
                    nicknamePanel.SetActive(true);
                    gameObject.SetActive(false);
                    return;
                }

                // Firebase 닉네임을 Photon에 등록
                PhotonNetwork.NickName = user.DisplayName;

                //네트워크 연결 시도
                PhotonNetwork.ConnectUsingSettings();

                //로딩 패널 표시
                NetworkManager.Instance.ShowLoading();  // 아래에 구현
            });
    }

    private void ResetPassword()
    {
        FirebaseManager.Auth.SendPasswordResetEmailAsync(idInput.text)
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsCanceled)
                {
                    Debug.LogError("비밀번호 재설정 이메일 발송 작업이 취소되었습니다.");
                    return;
                }
                if (task.IsFaulted)
                {
                    Debug.LogError($"비밀번호 재설정 이메일 발송 중 오류 발생: {task.Exception}");
                    return;
                }
                Debug.Log("비밀번호 재설정 이메일이 발송되었습니다.");
            });
    }
}
