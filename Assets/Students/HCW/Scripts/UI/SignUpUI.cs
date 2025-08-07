using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Firebase.Auth;
using Firebase.Extensions;

public class SignUpUI : BaseUI
{
    [SerializeField] private TMP_InputField emailInputField;
    [SerializeField] private TMP_InputField nameInputField;
    [SerializeField] private TMP_InputField passwordInputField;
    [SerializeField] private TMP_InputField confirmPasswordInputField;
    [SerializeField] private Button registerButton;
    [SerializeField] private Button backToLoginButton;
    [SerializeField] private TMP_Text messageText;

    private bool canName;
    private void Start()
    {
        registerButton.onClick.AddListener(OnRegisterButtonClicked);
        backToLoginButton.onClick.AddListener(ShowLoginUI);
        nameInputField.onValueChanged.AddListener(CheckNicknameSolid);
    }

    private void OnRegisterButtonClicked()
    {
        string email = emailInputField.text;
        string name = nameInputField.text;
        string password = passwordInputField.text;
        string confirmPassword = confirmPasswordInputField.text;

        if (password != confirmPassword)
        {
            SetMessage("비밀번호가 일치하지 않습니다.");
            return;
        }

        if (!canName)
        {
            SetMessage("닉네임을 확인해주세요");
            return;
        }

        SetMessage("회원가입 중..."); // 로딩 메시지 표시

        FirebaseManager.Auth.CreateUserWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled)
            {
                SetMessage("회원가입이 취소되었습니다.");
                return;
            }
            if (task.IsFaulted)
            {
                SetMessage("회원가입에 실패했습니다. 입력 정보를 확인해주세요.");
                Debug.LogError("회원가입 실패: " + task.Exception);
                return;
            }

            // 회원가입 성공
            Firebase.Auth.FirebaseUser newUser = task.Result.User;
            Debug.LogFormat("회원가입 성공: {0} ({1})", newUser.Email, newUser.UserId);

            // 이메일 인증 보내기
            newUser.SendEmailVerificationAsync().ContinueWithOnMainThread(verifyTask => {
                if(verifyTask.IsCompleted) {
                    SetMessage($"인증 메일을 {newUser.Email}로 발송했습니다.");
                }
            });

            // 사용자 프로필(닉네임) 업데이트
            UserProfile profile = new UserProfile { DisplayName = name };
            newUser.UpdateUserProfileAsync(profile).ContinueWithOnMainThread(profileTask => {
                if (profileTask.IsCanceled || profileTask.IsFaulted)
                {
                    SetMessage("닉네임 설정에 실패했습니다: " + profileTask.Exception.GetBaseException().Message);
                    return;
                }

                Debug.Log($"닉네임 설정 성공: {name}");
                FirebaseManager.UploadNickname(name);
                SetMessage("회원가입 및 닉네임 설정 완료!");

                // 모든 과정 성공 후 로그인 UI로 돌아가기
                ShowLoginUI();
            });
        });
    }

    private void ShowLoginUI()
    {
        Manager.UI.ReplaceUI(typeof(LoginUI));
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

    private void CheckNicknameSolid(string str)
    {
        if (str.Length <= 0)
        {
            SetMessage("닉네임을 입력해주세요");
            canName = false;
        }
        else if (str.Length > 12)
        {
            SetMessage("닉네임은 12글자 이하로 해주세요");
            canName = false;
        }
        else
        {
            SetMessage("");
            canName = true;
        }
    }
}
