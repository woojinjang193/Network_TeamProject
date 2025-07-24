using Firebase.Auth;
using Firebase.Extensions;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class EmailPanel : MonoBehaviour
{
    [SerializeField] GameObject loginPanel;
    [SerializeField] GameObject nicknamePanel;

    [SerializeField] Button backButton;

    private void Awake()
    {
        backButton.onClick.AddListener(Back);
    }

    private void OnEnable()
    {
        FirebaseManager.Auth.CurrentUser.SendEmailVerificationAsync()
            .ContinueWithOnMainThread(Task =>
        {
            if (Task.IsCanceled)
            {
                Debug.LogError("인증 이메일 발송 작업이 취소되었습니다.");
                return;
            }
            if (Task.IsFaulted)
            {
                Debug.LogError($"인증 이메일 발송 중 오류 발생: {Task.Exception}");
                return;
            }

            Debug.Log("인증 이메일 인증 성공");
            emailVerificaionRoutine = StartCoroutine(EmailVerificationRoutine());
        });
    }
    private void Back()
    {
        FirebaseManager.Auth.SignOut();
        loginPanel.SetActive(true);
        gameObject.SetActive(false);
    }

    Coroutine emailVerificaionRoutine;
    IEnumerator EmailVerificationRoutine()
    {
        FirebaseUser user = FirebaseManager.Auth.CurrentUser;
        WaitForSeconds delay = new WaitForSeconds(2f);

        while (true)
        {
            yield return delay;

            user.ReloadAsync();
            if (user.IsEmailVerified)
            {
                Debug.Log("이메일 인증 완료");
                nicknamePanel.SetActive(true);
                gameObject.SetActive(false);
                StopCoroutine(emailVerificaionRoutine);

            }
            else
            {
                Debug.Log("이메일 인증 대기 중...");
            }
        }

    }
}
