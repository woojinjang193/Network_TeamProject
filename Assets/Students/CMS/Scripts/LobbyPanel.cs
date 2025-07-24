using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Firebase.Auth;

public class LobbyPanel : MonoBehaviour
{
    [SerializeField] GameObject loginPanel;
    [SerializeField] GameObject editPanel;

    [SerializeField] TMP_Text emailText;
    [SerializeField] TMP_Text nameText;
    [SerializeField] TMP_Text userIdText;

    [SerializeField] Button logoutButton;
    [SerializeField] Button editProfileButton;

    private void Awake()
    {
        logoutButton.onClick.AddListener(Logout);
        editProfileButton.onClick.AddListener(EditProfile);
    }

    private void OnEnable()
    {
        FirebaseUser user = FirebaseManager.Auth.CurrentUser;
        emailText.text = user.Email;
        nameText.text = user.DisplayName;
        userIdText.text = user.UserId;
    }

    private void Logout()
    {
        FirebaseManager.Auth.SignOut();
        loginPanel.SetActive(true);
        gameObject.SetActive(false);
    }

    private void EditProfile()
    {
        editPanel.SetActive(true);
        gameObject.SetActive(false);
    }
}
