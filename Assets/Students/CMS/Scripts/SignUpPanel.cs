using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Firebase.Extensions;
using Firebase.Auth;

public class SignUpPanel : MonoBehaviour
{
    [SerializeField] GameObject loginPanel;

    [SerializeField] TMP_InputField idInput;
    [SerializeField] TMP_InputField passwordInput;
    [SerializeField] TMP_InputField passwordConfirmInput;

    [SerializeField] Button signUpButton;
    [SerializeField] Button cancelButton;

    private void Awake()
    {
        signUpButton.onClick.AddListener(SignUp);
        cancelButton.onClick.AddListener(Cancel);
    }

    private void SignUp()
    {
        if (passwordInput.text != passwordConfirmInput.text)
        {
            Debug.LogError("비밀번호가 일치하지 않습니다.");
            return;
        }
        FirebaseManager.Auth.CreateUserWithEmailAndPasswordAsync(idInput.text, passwordInput.text)
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsCanceled)
                {
                    Debug.LogError("회원가입 작업이 취소되었습니다.");
                    return;
                }
                if (task.IsFaulted)
                {
                    Debug.LogError($"회원가입 중 오류 발생: {task.Exception}");
                    return;
                }

                Debug.Log("회원가입 성공!");
                loginPanel.SetActive(true);
                gameObject.SetActive(false);
            });
    }

    private void Cancel()
    {
        loginPanel.SetActive(true);
        gameObject.SetActive(false);
    }

}
