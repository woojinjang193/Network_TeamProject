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
    [SerializeField] private Button exitButton;

    [Header("카테고리 패널")]
    [SerializeField] private GameObject graphicsContent;
    [SerializeField] private GameObject soundContent;
    [SerializeField] private GameObject controlsContent;

    [Header("그래픽 UI 요소")]
    [SerializeField] private TMP_Dropdown resolutionDropdown;
    [SerializeField] private Toggle fullscreenToggle; // 전체화면 토글 추가
    [SerializeField] private Button applyGraphicsButton;

    [Header("사운드 설정")] 
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Slider bgmVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;
    [SerializeField] private TextMeshProUGUI masterVolumeText;
    [SerializeField] private TextMeshProUGUI bgmVolumeText;
    [SerializeField] private TextMeshProUGUI sfxVolumeText;
    
    private Resolution[] resolutions;
    private const string RESOLUTION_WIDTH_PREF = "ResolutionWidth";
    private const string RESOLUTION_HEIGHT_PREF = "ResolutionHeight";
    private const string RESOLUTION_REFRESH_RATE_PREF = "ResolutionRefreshRate";
    private const string FULLSCREEN_PREF = "Fullscreen";


    private UIManager uiManager;

    protected void Awake()
    {

        uiManager = FindObjectOfType<UIManager>();

        // 탭 버튼에 리스너 추가
        graphicsTab.onClick.AddListener(() => ChangeTab(graphicsContent));
        soundTab.onClick.AddListener(() => ChangeTab(soundContent));
        //controlsTab.onClick.AddListener(() => ChangeTab(controlsContent));

        // 닫기 버튼에 리스너 추가
        closeButton.onClick.AddListener(OnCloseButtonClicked);

        // 종료 버튼에 리스너 추가
        exitButton.onClick.AddListener(OnClickExitButton);


        // --- 해상도 드롭다운 초기화 ---
        resolutions = Screen.resolutions;
        resolutionDropdown.ClearOptions();
        List<string> options = new List<string>();
        int currentResolutionIndex = 0;
        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = resolutions[i].width + " x " + resolutions[i].height;
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

        fullscreenToggle.onValueChanged.AddListener(OnFullscreenToggleChanged);

        SetDefaultSettings();
    }
    
    private void OnEnable() //로비로 돌아왔을 때, 전역적으로 다시 활성화 되는 것이 룸매니저 뿐임
    {
        if(Manager.Audio) Manager.Audio.VolumeLoad(); // 오디오 설정값을 불러와서 슬라이더 값 최신화
    }
    
    public void AudioInit()
    {
        // 슬라이더에 이벤트 추가
        masterVolumeSlider.onValueChanged.AddListener(Manager.Audio.SetMasterVolume);
        masterVolumeSlider.onValueChanged.AddListener(SetMasterVolumeText);
        bgmVolumeSlider.onValueChanged.AddListener(Manager.Audio.SetBGMVolume);
        bgmVolumeSlider.onValueChanged.AddListener(SetBGMVolumeText);
        sfxVolumeSlider.onValueChanged.AddListener(Manager.Audio.SetSFXVolume);
        sfxVolumeSlider.onValueChanged.AddListener(SetSFXVolumeText);
        
        // 저장된 사용자의 볼륨 설정 값 가져오기
        if (Manager.Audio.hasMasterVolume)
        {
            masterVolumeSlider.value = Manager.Audio.masterVolume;
        }
        else
        {
            masterVolumeSlider.value = 1;
        }

        if (Manager.Audio.hasBgmVolume)
        {
            bgmVolumeSlider.value = Manager.Audio.bgmVolume;
        }
        else
        {
            bgmVolumeSlider.value = 1;
        }

        if (Manager.Audio.hasSfxVolume)
        {
            sfxVolumeSlider.value = Manager.Audio.sfxVolume;
        }
        else
        {
            sfxVolumeSlider.value = 1;
        }
    }
    public override void Open()
    {
        gameObject.SetActive(true);
        // 기본으로 사운드 탭을 보여줌
        ChangeTab(soundContent);
        // 사운드 슬라이더를 저장된 값으로 설정

        // UI 열릴 때 설정 로드 및 UI 업데이트
        UpdateGraphicsUI(); // 그래픽 UI 업데이트 추가
        
    }

    public override void Close()
    {
        gameObject.SetActive(false);
    }

    private void ChangeTab(GameObject activeContent)
    {
        graphicsContent.SetActive(false);
        soundContent.SetActive(false);
        //controlsContent.SetActive(false);
        activeContent.SetActive(true);
    }

    private void OnCloseButtonClicked()
    {
        if (uiManager != null)
        {
            Manager.UI.PopUI();
        }
    }

    private void OnClickExitButton()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // 설정 로드/저장 및 UI 업데이트
    private void SetDefaultSettings()
    {
        // 기본 해상도 설정 (현재 해상도)
        if (!PlayerPrefs.HasKey(RESOLUTION_WIDTH_PREF)) PlayerPrefs.SetInt(RESOLUTION_WIDTH_PREF, Screen.currentResolution.width);
        if (!PlayerPrefs.HasKey(RESOLUTION_HEIGHT_PREF)) PlayerPrefs.SetInt(RESOLUTION_HEIGHT_PREF, Screen.currentResolution.height);
        if (!PlayerPrefs.HasKey(RESOLUTION_REFRESH_RATE_PREF)) PlayerPrefs.SetFloat(RESOLUTION_REFRESH_RATE_PREF, (float)Screen.currentResolution.refreshRateRatio.value);

        // 기본 전체화면 설정 (현재 Screen.fullScreen 값)
        if (!PlayerPrefs.HasKey(FULLSCREEN_PREF)) PlayerPrefs.SetInt(FULLSCREEN_PREF, Screen.fullScreen ? 1 : 0);
        
        PlayerPrefs.Save();
        
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

        // 현재 전체화면 설정을 토글에 표시
        fullscreenToggle.isOn = Screen.fullScreen;
    }

    private void OnResolutionChanged(int resolutionIndex)
    {
        // 드롭다운에서 선택된 해상도를 임시로 저장 (적용 버튼 누르기 전까지는 실제 적용 안 함)
        // 여기서는 바로 적용하지 않고, applyGraphicsButton 클릭 시 적용하도록 할 예정
    }

    private void OnApplyGraphicsButtonClicked()
    {
        Resolution selectedResolution = resolutions[resolutionDropdown.value];
        Screen.SetResolution(selectedResolution.width, selectedResolution.height, Screen.fullScreenMode, selectedResolution.refreshRateRatio);
        Debug.Log($"해상도 적용: {selectedResolution.width}x{selectedResolution.height}");

        // 적용 후 PlayerPrefs에 저장
        PlayerPrefs.SetInt(RESOLUTION_WIDTH_PREF, selectedResolution.width);
        PlayerPrefs.SetInt(RESOLUTION_HEIGHT_PREF, selectedResolution.height);
        PlayerPrefs.SetFloat(RESOLUTION_REFRESH_RATE_PREF, (float)selectedResolution.refreshRateRatio.value);
        PlayerPrefs.Save();
    }

    private void OnFullscreenToggleChanged(bool isOn)
    {
        // 전체화면 모드 변경
        Screen.fullScreen = isOn;
        Debug.Log($"전체화면 모드 변경: {isOn}");

        // 변경 후 PlayerPrefs에 저장
        PlayerPrefs.SetInt(FULLSCREEN_PREF, isOn ? 1 : 0);
        PlayerPrefs.Save();
    }

    private void SetMasterVolumeText(float value)
    {
        masterVolumeText.text = $"{(int)(value * 100)}";
    }

    private void SetBGMVolumeText(float value)
    {
        bgmVolumeText.text = $"{(int)(value * 100)}";
    }

    private void SetSFXVolumeText(float value)
    {
        sfxVolumeText.text = $"{(int)(value * 100)}";
    }
}

    
