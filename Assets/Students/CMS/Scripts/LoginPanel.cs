using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Firebase.Extensions;
using Firebase.Auth;

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
                if (task.IsCanceled)
                {
                    Debug.LogError("로그인 작업이 취소되었습니다.");
                    return;
                }
                if (task.IsFaulted)
                {
                    Debug.LogError($"로그인 중 오류 발생: {task.Exception}");
                    return;
                }
                Debug.Log("로그인 성공!");

                FirebaseUser user = task.Result.User;
                if(user.IsEmailVerified == true)
                {
                    if(user.DisplayName == "")
                    {
                        nicknamePanel.SetActive(true);
                    }
                    else
                    {
                        lobbyPanel.SetActive(true);
                    }
                    gameObject.SetActive(false);
                }
                else
                {
                    emailPanel.SetActive(true);
                    gameObject.SetActive(false); 
                }
  
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
