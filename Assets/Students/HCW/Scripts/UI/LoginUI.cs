using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoginUI : BaseUI
{
    [SerializeField] private TMP_InputField nameInputField;
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
        string id = nameInputField.text;
        string password = passwordInputField.text;

        SetMessage("로그인 중..."); // 로딩 메시지 표시

        // TODO: 여기에 Firebase 로그인 로직을 추가할 예정
        Debug.Log($"로그인 시도: {id} / {password}");
    }

    private void ShowSignUpUI()
    {
        FindObjectOfType<UIManager>().ShowUI(typeof(SignUpUI));
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