using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SignUpUI : BaseUI
{
    [SerializeField] private TMP_InputField emailInputField;
    [SerializeField] private TMP_InputField nameInputField;
    [SerializeField] private TMP_InputField passwordInputField;
    [SerializeField] private TMP_InputField confirmPasswordInputField;
    [SerializeField] private Button registerButton;
    [SerializeField] private Button backToLoginButton;
    [SerializeField] private TMP_Text messageText;

    private void Start()
    {
        registerButton.onClick.AddListener(OnRegisterButtonClicked);
        backToLoginButton.onClick.AddListener(ShowLoginUI);
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

        SetMessage("회원가입 중..."); // 로딩 메시지 표시

        // TODO: 여기에 Firebase 회원가입 로직을 추가할 예정
        Debug.Log($"회원가입 시도: {email} / {name} / {password}");
    }

    private void ShowLoginUI()
    {
        FindObjectOfType<UIManager>().ShowUI(typeof(LoginUI));
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