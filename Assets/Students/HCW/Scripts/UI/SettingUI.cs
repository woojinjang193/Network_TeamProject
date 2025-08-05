using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingUI : BaseUI
{
    [Header("카테고리")]
    [SerializeField] private Button soundTab;
    [SerializeField] private Button graphicsTab;
    [SerializeField] private Button controlsTab;
    [SerializeField] private Button closeButton; // 닫기 버튼
     
    [Header("Contents")]
    [SerializeField] private GameObject graphicsContent;
    [SerializeField] private GameObject soundContent;
    [SerializeField] private GameObject controlsContent;

    

    private UIManager uiManager;

    private void Awake()
    {
        uiManager = FindObjectOfType<UIManager>();

        // 탭 버튼에 리스너 추가
        graphicsTab.onClick.AddListener(() => ChangeTab(graphicsContent));
        soundTab.onClick.AddListener(() => ChangeTab(soundContent));
        controlsTab.onClick.AddListener(() => ChangeTab(controlsContent));

        // 닫기 버튼에 리스너 추가
        closeButton.onClick.AddListener(OnCloseButtonClicked);
    }

    public override void Open()
    {
        gameObject.SetActive(true);
        // 기본으로 사운드 탭을 보여줌
        ChangeTab(soundContent);
    }

    public override void Close()
    {
        // TODO: 만약 변경사항이 저장되지 않았다면 팝업을 띄우는 로직 추가 가능
        gameObject.SetActive(false);
    }

    private void ChangeTab(GameObject activeContent)
    {
        graphicsContent.SetActive(false);
        soundContent.SetActive(false);
        controlsContent.SetActive(false);

        activeContent.SetActive(true);
    }

    private void OnCloseButtonClicked()
    {
        if (uiManager != null)
        {
            UIManager.Instance.PopUI();
        }
    }
}
