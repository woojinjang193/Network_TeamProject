using Firebase.Auth;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EditPanel : MonoBehaviour
{
    [SerializeField] GameObject lobbyPanel;

    [SerializeField] TMP_InputField nameInputField;
    [SerializeField] TMP_InputField passInputField;
    [SerializeField] TMP_InputField passConfirmInputField;

    [SerializeField] TMP_Text emailText;
    [SerializeField] TMP_Text userIdText;


    [SerializeField] Button nicknameCorfirmButton;
    [SerializeField] Button passConfirmButton;
    [SerializeField] Button backButton;

    private void Awake()
    {
        nicknameCorfirmButton.onClick.AddListener(ChangeNickname);
        passConfirmButton.onClick.AddListener(ChangePassword);
        backButton.onClick.AddListener(Back);
    }

    private void OnEnable()
    {
        FirebaseUser user = FirebaseManager.Auth.CurrentUser;
        emailText.text = user.Email;
        nameInputField.text = user.DisplayName;
        userIdText.text = user.UserId;
    }

    private void ChangeNickname()
    {
        UserProfile profile = new UserProfile();
        profile.DisplayName = nameInputField.text;

        FirebaseUser user = FirebaseManager.Auth.CurrentUser;
        user.UpdateUserProfileAsync(profile)
            .ContinueWith(task =>
        {
            if (task.IsCanceled)
            {
                Debug.LogError("닉네임 변경 취소");
                return;
            }
            if (task.IsFaulted)
            {
                Debug.LogError($"닉네임 변경 실패: {task.Exception}");
                return;
            }
            Debug.Log("닉네임 변경 성공: ");

        });

    }

    private void ChangePassword()
    {
        if (passInputField.text != passConfirmInputField.text)
        {
            Debug.LogError("비밀번호가 일치하지 않습니다.");
            return;
        }
        FirebaseUser user = FirebaseManager.Auth.CurrentUser;
        user.UpdatePasswordAsync(passInputField.text)
            .ContinueWith(task =>
        {
            if (task.IsCanceled)
            {
                Debug.LogError("비밀번호 변경 취소");
                return;
            }
            if (task.IsFaulted)
            {
                Debug.LogError($"비밀번호 변경 실패: {task.Exception}");
                return;
            }
            Debug.Log("비밀번호 변경 성공: ");
        });

    }

    private void Back()
    {
        lobbyPanel.SetActive(true);
        gameObject.SetActive(false);
    }
}
