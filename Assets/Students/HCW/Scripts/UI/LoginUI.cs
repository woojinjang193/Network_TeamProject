using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Firebase.Auth;
using Firebase.Extensions;
using Photon.Pun;

public class LoginUI : BaseUI
{
    [SerializeField] private TMP_InputField emailInputField;
    [SerializeField] private TMP_InputField passwordInputField;
    [SerializeField] private Button loginButton;
    [SerializeField] private Button showSignUpUIButton;
    [SerializeField] private TMP_Text messageText;

    private float loginTimer;
    private void Start()
    {
        loginTimer = 2f;
        loginButton.onClick.AddListener(OnLoginButtonClicked);
        showSignUpUIButton.onClick.AddListener(ShowSignUpUI);
    }

    private void OnLoginButtonClicked()
    {
        string email = emailInputField.text;
        string password = passwordInputField.text;

        SetMessage("로그인 중...");

        FirebaseManager.Auth.SignInWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled || task.IsFaulted)
            {
                SetMessage("로그인에 실패했습니다: " + task.Exception.GetBaseException().Message);
                Debug.Log($"로그인 실패: {task.Exception}");
                return;
            }

            FirebaseUser user = task.Result.User;
            Debug.Log("로그인 성공");

            if (!user.IsEmailVerified)
            {
                SetMessage("이메일 인증이 필요합니다.");
                Debug.Log("이메일 인증 안 됨 -> 이메일 인증 UI로 전환해야 함");
                return;
            }

            if (string.IsNullOrEmpty(user.DisplayName))
            {
                SetMessage("닉네임 설정이 필요합니다.");
                Debug.Log("닉네임 설정 안 됨 -> 닉네임 설정 UI로 전환해야 함");
                return;
            }

            // Photon 닉네임 설정
            Photon.Pun.PhotonNetwork.NickName = user.DisplayName;
            Debug.Log($"Photon 닉네임 설정 완료: {user.DisplayName}");

            // Photon 서버 연결 및 로비 이동
            while (!Photon.Pun.PhotonNetwork.IsConnected)
            {
                loginTimer += Time.deltaTime;
                if (loginTimer >= 2f)
                {
                    Debug.Log("Photon 서버에 연결되지 않음. 연결 시도 중...");
                    Photon.Pun.PhotonNetwork.ConnectUsingSettings();
                    loginTimer = 0f;
                }
            }
            //PhotonNetwork.JoinLobby(); // 로그인 성공 후 로비로 이동
        });
    }

    private void ShowSignUpUI()
    {
        Manager.UI.ReplaceUI(typeof(SignUpUI));
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
