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
        Debug.Log("로그인 버튼 클릭됨");

        string email = idInput.text;
        string password = passwordInput.text;

        Debug.Log($"입력된 이메일: {email}, 비밀번호 길이: {password.Length}");

        FirebaseManager.Auth.SignInWithEmailAndPasswordAsync(email, password)
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsCanceled || task.IsFaulted)
                {
                    Debug.LogError($"로그인 실패: {task.Exception}");
                    return;
                }

                FirebaseUser user = task.Result.User;
                Debug.Log("로그인 성공");
                Debug.Log($"이메일 인증 여부: {user.IsEmailVerified}");
                Debug.Log($"닉네임(DisplayName): {user.DisplayName}");

                if (!user.IsEmailVerified)
                {
                    Debug.Log("이메일 인증 안 됨 이메일 인증 패널로 전환");
                    emailPanel.SetActive(true);
                    gameObject.SetActive(false);
                    return;
                }

                if (string.IsNullOrEmpty(user.DisplayName))
                {
                    Debug.Log("닉네임이 설정되지 않음 닉네임 설정 패널로 전환");
                    nicknamePanel.SetActive(true);
                    gameObject.SetActive(false);
                    return;
                }

                PhotonNetwork.NickName = user.DisplayName;
                Debug.Log("Photon 닉네임 설정 완료");

                if (!PhotonNetwork.IsConnected)
                {
                    Debug.Log("Photon 서버에 연결되지 않음 연결 시도 중");
                    PhotonNetwork.ConnectUsingSettings();
                    NetworkManager.Instance.ShowLoading();
                }
                else
                {
                    Debug.Log("Photon 서버에 이미 연결됨 로비로 바로 이동");
                    NetworkManager.Instance.ShowLobby();
                }

                gameObject.SetActive(false);
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
