using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Firebase.Auth;
using Firebase.Extensions;

public class LoginUI : BaseUI
{
    [SerializeField] private TMP_InputField emailInputField;
    [SerializeField] private TMP_InputField passwordInputField;
    [SerializeField] private Button loginButton;
    [SerializeField] private Button showSignUpUIButton;
    [SerializeField] private TMP_Text messageText;

    private void Start()
    {
        loginButton.onClick.AddListener(OnLoginButtonClicked);
        showSignUpUIButton.onClick.AddListener(ShowSignUpUI);
    }

    private void OnLoginButtonClicked()
    {
        string email = emailInputField.text;
        string password = passwordInputField.text;

        SetMessage("로그인 중..."); // 로딩 메시지 표시

        FirebaseManager.Auth.SignInWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled)
            {
                SetMessage("로그인이 취소되었습니다.");
                return;
            }
            if (task.IsFaulted)
            {
                SetMessage("아이디 또는 비밀번호가 틀렸습니다.");
                Debug.LogError("SignInWithEmailAndPasswordAsync error: " + task.Exception);
                return;
            }

            // 로그인 성공
            Firebase.Auth.FirebaseUser newUser = task.Result.User;
            SetMessage($"로그인 성공: {newUser.Email}");
            Debug.LogFormat("로그인 성공: {0} ({1})", newUser.DisplayName, newUser.UserId);

            // 로그인 성공 후 로비 UI 표시
            UIManager.Instance.ReplaceUI(typeof(LobbyUI));
        });
    }

    private void ShowSignUpUI()
    {
        UIManager.Instance.ReplaceUI(typeof(SignUpUI));
    }

    // 호출할 메시지 업데이트 함수
    public void SetMessage(string message)
    {
        if (messageText != null)
        {
            messageText.text = message;
        }
    }

    public override void Open()
    {
        SetMessage(""); // 패널이 열릴 때 메시지 초기화
        gameObject.SetActive(true);
    }

    public override void Close()
    {
        gameObject.SetActive(false);
    }
}