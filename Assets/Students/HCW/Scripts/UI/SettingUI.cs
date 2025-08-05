using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingUI : BaseUI
{
    [Header("카테고리")]
    [SerializeField] private Button soundTab;
    [SerializeField] private Button graphicsTab;
    [SerializeField] private Button controlsTab;
    [SerializeField] private Button closeButton;

    [Header("카테고리 패널")]
    [SerializeField] private GameObject graphicsContent;
    [SerializeField] private GameObject soundContent;
    [SerializeField] private GameObject controlsContent;

    [Header("그래픽 UI 요소")]
    [SerializeField] private TMPro.TMP_Dropdown resolutionDropdown;
    [SerializeField] private Button applyGraphicsButton;

    private Resolution[] resolutions;
    private const string RESOLUTION_WIDTH_PREF = "ResolutionWidth";
    private const string RESOLUTION_HEIGHT_PREF = "ResolutionHeight";
    private const string RESOLUTION_REFRESH_RATE_PREF = "ResolutionRefreshRate";

    [Header("컨트롤 UI 요소")]
    [SerializeField] private Slider mouseSensitivitySlider;
    [SerializeField] private TextMeshProUGUI mouseSensitivityValueText; // 슬라이더 값 표시용
    [SerializeField] private TMP_InputField upKeyInputField;
    [SerializeField] private TMP_InputField downKeyInputField;
    [SerializeField] private TMP_InputField leftKeyInputField;
    [SerializeField] private TMP_InputField rightKeyInputField;
    [SerializeField] private TMP_InputField squidKeyInputField;
    [SerializeField] private TMP_InputField jumpKeyInputField;


    private Dictionary<string, KeyCode> keyBindings = new Dictionary<string, KeyCode>();
    private float mouseSensitivity;

    private const string UP_KEY_PREF = "UpKey";
    private const string DOWN_KEY_PREF = "DownKey";
    private const string LEFT_KEY_PREF = "LeftKey";
    private const string RIGHT_KEY_PREF = "RightKey";
    private const string SQUID_KEY_PREF = "SquidKey";
    private const string JUMP_KEY_PREF = "JumpKey";
    private const string MOUSE_SENSITIVITY_PREF = "MouseSensitivity";

    private UIManager uiManager;

    protected void Awake()
    {

        uiManager = FindObjectOfType<UIManager>();

        // 탭 버튼에 리스너 추가
        graphicsTab.onClick.AddListener(() => ChangeTab(graphicsContent));
        soundTab.onClick.AddListener(() => ChangeTab(soundContent));
        controlsTab.onClick.AddListener(() => ChangeTab(controlsContent));

        // 닫기 버튼에 리스너 추가
        closeButton.onClick.AddListener(OnCloseButtonClicked);

        // --- InputField 및 Slider 리스너 추가 ---
        upKeyInputField.onEndEdit.AddListener((value) => OnKeyInputEndEdit(UP_KEY_PREF, value));
        downKeyInputField.onEndEdit.AddListener((value) => OnKeyInputEndEdit(DOWN_KEY_PREF, value));
        leftKeyInputField.onEndEdit.AddListener((value) => OnKeyInputEndEdit(LEFT_KEY_PREF, value));
        rightKeyInputField.onEndEdit.AddListener((value) => OnKeyInputEndEdit(RIGHT_KEY_PREF, value));
        squidKeyInputField.onEndEdit.AddListener((value) => OnKeyInputEndEdit(SQUID_KEY_PREF, value));
        jumpKeyInputField.onEndEdit.AddListener((value) => OnKeyInputEndEdit(JUMP_KEY_PREF, value));

        mouseSensitivitySlider.onValueChanged.AddListener(OnMouseSensitivityChanged);

        // --- 해상도 드롭다운 초기화 ---
        resolutions = Screen.resolutions;
        resolutionDropdown.ClearOptions();
        List<string> options = new List<string>();
        int currentResolutionIndex = 0;
        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = resolutions[i].width + " x " + resolutions[i].height + " @ " + resolutions[i].refreshRateRatio.value + "Hz";
            options.Add(option);

            if (resolutions[i].width == Screen.currentResolution.width &&
                resolutions[i].height == Screen.currentResolution.height &&
                resolutions[i].refreshRateRatio.value == Screen.currentResolution.refreshRateRatio.value)
            {
                currentResolutionIndex = i;
            }
        }
        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = currentResolutionIndex;
        resolutionDropdown.RefreshShownValue();

        resolutionDropdown.onValueChanged.AddListener(OnResolutionChanged);
        applyGraphicsButton.onClick.AddListener(OnApplyGraphicsButtonClicked);

        // 초기 키 설정
        SetDefaultKeyBindings();
    }

    public override void Open()
    {
        gameObject.SetActive(true);
        // 기본으로 사운드 탭을 보여줌
        ChangeTab(soundContent);

        // UI 열릴 때 설정 로드 및 UI 업데이트
        LoadSettings();
        UpdateControlsUI();
        UpdateGraphicsUI(); // 그래픽 UI 업데이트 추가
    }

    public override void Close()
    {
        gameObject.SetActive(false);
        // UI 닫힐 때 설정 저장
        SaveSettings();
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
            Manager.UI.PopUI();
        }
    }

    // 설정 로드/저장 및 UI 업데이트
    private void SetDefaultKeyBindings()
    {
        if (!PlayerPrefs.HasKey(UP_KEY_PREF)) PlayerPrefs.SetString(UP_KEY_PREF, KeyCode.W.ToString());
        if (!PlayerPrefs.HasKey(DOWN_KEY_PREF)) PlayerPrefs.SetString(DOWN_KEY_PREF, KeyCode.S.ToString());
        if (!PlayerPrefs.HasKey(LEFT_KEY_PREF)) PlayerPrefs.SetString(LEFT_KEY_PREF, KeyCode.A.ToString());
        if (!PlayerPrefs.HasKey(RIGHT_KEY_PREF)) PlayerPrefs.SetString(RIGHT_KEY_PREF, KeyCode.D.ToString());
        if (!PlayerPrefs.HasKey(SQUID_KEY_PREF)) PlayerPrefs.SetString(SQUID_KEY_PREF, KeyCode.LeftShift.ToString());
        if (!PlayerPrefs.HasKey(JUMP_KEY_PREF)) PlayerPrefs.SetString(JUMP_KEY_PREF, KeyCode.Space.ToString());
        if (!PlayerPrefs.HasKey(MOUSE_SENSITIVITY_PREF)) PlayerPrefs.SetFloat(MOUSE_SENSITIVITY_PREF, 1.0f); // 기본 감도

        // 기본 해상도 설정 (현재 해상도)
        if (!PlayerPrefs.HasKey(RESOLUTION_WIDTH_PREF)) PlayerPrefs.SetInt(RESOLUTION_WIDTH_PREF, Screen.currentResolution.width);
        if (!PlayerPrefs.HasKey(RESOLUTION_HEIGHT_PREF)) PlayerPrefs.SetInt(RESOLUTION_HEIGHT_PREF, Screen.currentResolution.height);
        if (!PlayerPrefs.HasKey(RESOLUTION_REFRESH_RATE_PREF)) PlayerPrefs.SetFloat(RESOLUTION_REFRESH_RATE_PREF, Screen.currentResolution.refreshRateRatio.value);
        
        PlayerPrefs.Save();
    }

    private void LoadSettings()
    {
        keyBindings[UP_KEY_PREF] = (KeyCode)Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString(UP_KEY_PREF));
        keyBindings[DOWN_KEY_PREF] = (KeyCode)Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString(DOWN_KEY_PREF));
        keyBindings[LEFT_KEY_PREF] = (KeyCode)Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString(LEFT_KEY_PREF));
        keyBindings[RIGHT_KEY_PREF] = (KeyCode)Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString(RIGHT_KEY_PREF));
        keyBindings[SQUID_KEY_PREF] = (KeyCode)Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString(SQUID_KEY_PREF));
        keyBindings[JUMP_KEY_PREF] = (KeyCode)Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString(JUMP_KEY_PREF));
        mouseSensitivity = PlayerPrefs.GetFloat(MOUSE_SENSITIVITY_PREF);
    }

    private void SaveSettings()
    {
        PlayerPrefs.SetString(UP_KEY_PREF, keyBindings[UP_KEY_PREF].ToString());
        PlayerPrefs.SetString(DOWN_KEY_PREF, keyBindings[DOWN_KEY_PREF].ToString());
        PlayerPrefs.SetString(LEFT_KEY_PREF, keyBindings[LEFT_KEY_PREF].ToString());
        PlayerPrefs.SetString(RIGHT_KEY_PREF, keyBindings[RIGHT_KEY_PREF].ToString());
        PlayerPrefs.SetString(SQUID_KEY_PREF, keyBindings[SQUID_KEY_PREF].ToString());
        PlayerPrefs.SetString(JUMP_KEY_PREF, keyBindings[JUMP_KEY_PREF].ToString());
        PlayerPrefs.SetFloat(MOUSE_SENSITIVITY_PREF, mouseSensitivity);

        // 해상도 저장
        PlayerPrefs.SetInt(RESOLUTION_WIDTH_PREF, Screen.currentResolution.width);
        PlayerPrefs.SetInt(RESOLUTION_HEIGHT_PREF, Screen.currentResolution.height);
        PlayerPrefs.SetFloat(RESOLUTION_REFRESH_RATE_PREF, Screen.currentResolution.refreshRateRatio.value);
        
        PlayerPrefs.Save();
    }

    private void UpdateControlsUI()
    {
        upKeyInputField.text = keyBindings[UP_KEY_PREF].ToString();
        downKeyInputField.text = keyBindings[DOWN_KEY_PREF].ToString();
        leftKeyInputField.text = keyBindings[LEFT_KEY_PREF].ToString();
        rightKeyInputField.text = keyBindings[RIGHT_KEY_PREF].ToString();
        squidKeyInputField.text = keyBindings[SQUID_KEY_PREF].ToString();
        jumpKeyInputField.text = keyBindings[JUMP_KEY_PREF].ToString();

        mouseSensitivitySlider.value = mouseSensitivity;
        mouseSensitivityValueText.text = mouseSensitivity.ToString("F2");
    }

    private void UpdateGraphicsUI()
    {
        // 현재 설정된 해상도를 드롭다운에 표시
        int currentResolutionIndex = 0;
        for (int i = 0; i < resolutions.Length; i++)
        {
            if (resolutions[i].width == Screen.width &&
                resolutions[i].height == Screen.height &&
                resolutions[i].refreshRateRatio.value == Screen.currentResolution.refreshRateRatio.value) // 현재 적용된 해상도와 일치하는지 확인
            {
                currentResolutionIndex = i;
                break;
            }
        }
        resolutionDropdown.value = currentResolutionIndex;
        resolutionDropdown.RefreshShownValue();
    }

    private void OnResolutionChanged(int resolutionIndex)
    {
        // 드롭다운에서 선택된 해상도를 임시로 저장 (적용 버튼 누르기 전까지는 실제 적용 안 함)
        // 여기서는 바로 적용하지 않고, applyGraphicsButton 클릭 시 적용하도록 할 예정
    }

    private void OnApplyGraphicsButtonClicked()
    {
        Resolution selectedResolution = resolutions[resolutionDropdown.value];
        Screen.SetResolution(selectedResolution.width, selectedResolution.height, Screen.fullScreen, selectedResolution.refreshRateRatio);
        Debug.Log($"해상도 적용: {selectedResolution.width}x{selectedResolution.height} @ {selectedResolution.refreshRateRatio.value}Hz");

        // 적용 후 PlayerPrefs에 저장
        PlayerPrefs.SetInt(RESOLUTION_WIDTH_PREF, selectedResolution.width);
        PlayerPrefs.SetInt(RESOLUTION_HEIGHT_PREF, selectedResolution.height);
        PlayerPrefs.SetFloat(RESOLUTION_REFRESH_RATE_PREF, selectedResolution.refreshRateRatio.value);
        PlayerPrefs.Save();
    }
}
