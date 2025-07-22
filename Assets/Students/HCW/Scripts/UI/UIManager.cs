using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField] private List<BaseUI> uiPanels;
    [SerializeField] private BaseUI startPanel;
    private BaseUI currentActivePanel;

    // 시작시 모든 UI 패널 비활성화
    private void Awake()
    {
        foreach (var panel in uiPanels)
        {
            panel.Close();
        }
    }

    private void Start()
    {
        // 시작 패널을 활성화
        if (startPanel != null)
        {
            ShowUI(startPanel.GetType());
        }
    }

    // UI 활성화 메서드
    public void ShowUI(System.Type uiType)
    {
        // 중복 방지
        if (currentActivePanel != null && currentActivePanel.GetType() == uiType)
        {
            return;
        }
        // 열고자 하는 UI 타입과 일치하는 패널을 리스트에서 찾음
        BaseUI uiToShow = uiPanels.FirstOrDefault(panel => panel.GetType() == uiType);

        if (uiToShow != null)
        {
            if (currentActivePanel != null)
            {
                currentActivePanel.Close();
            }
            // 새 UI을 열고 현재 활성화 패널로 등록
            uiToShow.Open();
            currentActivePanel = uiToShow;
        }
    }

    // 열려있는 UI 비활성화 메서드
    public void CloseCurrentUI()
    {
        if (currentActivePanel != null)
        {
            currentActivePanel.Close();
            currentActivePanel = null;
        }
    }
}