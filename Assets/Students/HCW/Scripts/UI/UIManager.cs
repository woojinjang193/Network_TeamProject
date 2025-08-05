using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UIManager : Singleton<UIManager>
{
    public BaseUI CurrentUI => uiStack.Count > 0 ? uiStack.Peek() : null;

    private Dictionary<Type, BaseUI> uiDictionary = new Dictionary<Type, BaseUI>();
    private Stack<BaseUI> uiStack = new Stack<BaseUI>();

    [SerializeField] private BaseUI startPanel; // 시작 UI만 인스펙터에서 설정

    protected override void Awake()
    {
        base.Awake();

        // 씬에 있는 모든 BaseUI 상속 객체를 찾아 비활성화하고 딕셔너리에 등록
        BaseUI[] allUIs = FindObjectsOfType<BaseUI>(true);
        foreach (var ui in allUIs)
        {
            ui.gameObject.SetActive(false);
            if (!uiDictionary.ContainsKey(ui.GetType()))
            {
                uiDictionary.Add(ui.GetType(), ui);
                Debug.Log($"UIManager: {ui.GetType().Name} UI를 등록했습니다.");
            }
        }
    }

    private void Start()
    {
        if (startPanel != null)
        {
            ReplaceUI(startPanel.GetType());
        }
        else if (uiDictionary.Count > 0)
        {
            // 시작 패널이 지정되지 않았다면 찾은 UI 중 첫 번째 UI를 시작 패널로 사용
            ReplaceUI(uiDictionary.Keys.First());
        }
    }

    private void Update()
    {
        // ESC 키로 설정 UI 토글
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // SettingUI가 등록되어 있는지 확인
            if (!uiDictionary.ContainsKey(typeof(SettingUI)))
            {
                Debug.LogWarning("UIManager에 SettingUI가 등록되지 않았습니다.");
                return;
            }

            // 현재 UI가 SettingUI라면 닫고, 아니라면 연다.
            if (CurrentUI != null && CurrentUI.GetType() == typeof(SettingUI))
            {
                PopUI();
            }
            else
            {
                PushUI(typeof(SettingUI));
            }
        }
    }

    // 현재 UI를 완전히 교체 (이전 UI는 스택에서 제거)
    public void ReplaceUI(Type uiType)
    {
        if (!uiDictionary.ContainsKey(uiType))
        {
            Debug.LogError($"{uiType.Name}은(는) UIManager에 등록되지 않았습니다.");
            return;
        }

        // 현재 열려있는 UI가 있다면 스택에서 제거하고 닫음
        if (uiStack.Count > 0)
        {
            BaseUI currentUI = uiStack.Pop();
            currentUI.Close();
        }

        // 새 UI를 열고 스택에 추가
        BaseUI uiToShow = uiDictionary[uiType];
        uiToShow.Open();
        uiStack.Push(uiToShow);
    }

    // 새 UI를 스택에 추가하고 이전 UI는 비활성화 (이전 UI로 돌아갈 수 있음)
    public void PushUI(Type uiType)
    {
        if (!uiDictionary.ContainsKey(uiType))
        {
            Debug.LogError($"{uiType.Name}은(는) UIManager에 등록되지 않았습니다.");
            return;
        }

        // 현재 열려있는 UI가 있다면 비활성화
        if (uiStack.Count > 0)
        {
            BaseUI currentUI = uiStack.Peek();
            currentUI.Close();
        }

        // 새 UI를 열고 스택에 추가
        BaseUI uiToPush = uiDictionary[uiType];
        uiToPush.Open();
        uiStack.Push(uiToPush);
    }

    // 현재 UI를 닫고 스택의 이전 UI를 활성화 (뒤로가기 기능)
    public void PopUI()
    {
        if (uiStack.Count > 0)
        {
            BaseUI uiToPop = uiStack.Pop();
            uiToPop.Close();

            // 스택에 다른 UI가 남아있다면, 가장 위의 UI를 다시 활성화
            if (uiStack.Count > 0)
            {
                BaseUI previousUI = uiStack.Peek();
                previousUI.Open();
            }
            else
            {
                Debug.LogWarning("스택에 더 이상 UI가 없습니다. 게임을 종료하거나 기본 UI를 표시하세요.");
                // 여기에 게임 종료 또는 기본 UI 표시 로직 추가 가능
            }
        }
        else
        {
            Debug.LogWarning("스택에 UI가 없습니다.");
        }
    }
}
