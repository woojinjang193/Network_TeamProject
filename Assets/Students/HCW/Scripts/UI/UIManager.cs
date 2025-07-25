using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField] private List<BaseUI> uiPanels;
    [SerializeField] private BaseUI startPanel;

    private Stack<BaseUI> uiStack = new Stack<BaseUI>();

    private void Awake()
    {
        // 모든 UI 패널을 비활성화 상태로 초기화
        foreach (var panel in uiPanels)
        {
            panel.gameObject.SetActive(false);
        }
    }

    private void Start()
    {
        if (startPanel != null)
        {
            ShowUI(startPanel.GetType());
        }
    }

    // 새 UI를 열고 스택에 추가
    public void ShowUI(System.Type uiType)
    {
        // 현재 활성화된 UI가 있다면 비활성화
        if (uiStack.Count > 0)
        {
            BaseUI currentUI = uiStack.Peek();
            if (currentUI.GetType() == uiType) return; // 이미 열려있으면 무시
            currentUI.Close();
        }

        BaseUI uiToShow = uiPanels.FirstOrDefault(panel => panel.GetType() == uiType);

        if (uiToShow != null)
        {
            uiToShow.Open();
            uiStack.Push(uiToShow);
        }
    }

    // 현재 UI를 닫고 이전 UI를 다시 활성화
    public void CloseCurrentUI()
    {
        if (uiStack.Count > 0)
        {
            BaseUI uiToClose = uiStack.Pop();
            uiToClose.Close();

            // 스택에 다른 UI가 남아있다면, 가장 위의 UI를 다시 활성화
            if (uiStack.Count > 0)
            {
                BaseUI previousUI = uiStack.Peek();
                previousUI.Open();
            }
        }
    }
}
