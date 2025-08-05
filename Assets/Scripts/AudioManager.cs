using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

public class AudioManager : Singleton<AudioManager>
{
    // 오디오 데이터를 모아놓은 데이터베이스
    private AudioDataBase audioDB;


    // 기본음 세팅
    private AudioData defaultBGM; // 별도의 명령이 없을 경우 재생하는 배경음악
    private AudioData defaultAmbient; // '' 백색소음

    // 믹서 세팅
    private AudioMixer mixer;  // 볼륨을 관리하는 오디오 믹서
    private AudioMixerGroup masterGroup;
    private AudioMixerGroup bgmGroup;
    private AudioMixerGroup sfxGroup;
    
    private AudioSource bgmSource; // 배경음악 오디오 소스
    private AudioSource ambientSource; // 게임에서 백색소음에 해당하는 오디오 소스
    private AudioSource effectSource; // 효과음 오디오 소스
    
    
    private Coroutine bgmFadeRoutine;
    private Coroutine ambFadeRoutine;
    private Coroutine stopRoutine;
    private Coroutine startBgmRoutine;
    private Coroutine startAmbRoutine;
    
    private Dictionary<string, AudioData> audioDict = new();

    // 테스트 볼륨
    [Header("Set UI Ref")] 
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider sfxSlider;

    protected override void Awake()
    {
        base.Awake();
        
        // 오디오 데이터베이스 연결
        audioDB = Resources.Load<AudioDataBase>($"Audio/AudioDB");
        
        // LINQ.Enumerable의 ToDictionary 기능 사용
        audioDict = audioDB.audioList.ToDictionary(s => s.clipName, s => s);

        // 믹서 및 그룹 연결
        mixer = Resources.Load<AudioMixer>("Audio/AudioMixer");
        bgmGroup = mixer.FindMatchingGroups("Master/BGM")[0];
        sfxGroup = mixer.FindMatchingGroups("Master/SFX")[0];
        
        
        // 오디오 소스 세팅
        bgmSource = gameObject.GetOrAddComponent<AudioSource>();
        ambientSource = gameObject.AddComponent<AudioSource>();
        effectSource = gameObject.AddComponent<AudioSource>();
        
        // 오디오 소스와 믹서 연결
        bgmSource.outputAudioMixerGroup = bgmGroup;
        ambientSource.outputAudioMixerGroup = bgmGroup;
        effectSource.outputAudioMixerGroup = sfxGroup;
        
        
        // 기본 브금 세팅
        defaultBGM = audioDict.TryGetValue("defaultBGM", out defaultBGM) ? defaultBGM : audioDB.audioList[0];
        defaultAmbient = audioDict.TryGetValue("defaultAmbient", out defaultAmbient) ? defaultAmbient : audioDB.audioList[0];
    }
    
    private void Start()
    {
        // 사용자 설정 가져오기
        VolumeLoad();
        // 기본 백색소음 재생
        SwitchAmbient("defaultAmbient",1f);
        // 기본 배경음악 재생
        SwitchBGM("defaultBGM",1f);
    }

    private void VolumeLoad() // 사용자 설정을 불러오기
    {
        float value;
        if (PlayerPrefs.HasKey("MasterVolume"))
        {
            value = PlayerPrefs.GetFloat("MasterVolume");
            mixer.SetFloat("MasterVolume", Mathf.Log10(Mathf.Clamp(value,0.0001f,1f))*20);
        }

        if (PlayerPrefs.HasKey("BGMVolume"))
        {
            value = PlayerPrefs.GetFloat("BGMVolume");
            mixer.SetFloat("BGMVolume", Mathf.Log10(Mathf.Clamp(value,0.0001f,1f))*20);
        }

        if (PlayerPrefs.HasKey("SFXVolume"))
        {
            value = PlayerPrefs.GetFloat("SFXVolume");
            mixer.SetFloat("SFXVolume", Mathf.Log10(Mathf.Clamp(value,0.0001f,1f))*20);
        }
    }
    
    public AudioSource PlayClip(string clipName, Vector3 pos) // 인게임 오디오 클립 재생
    {
        if (!audioDict.TryGetValue(clipName, out AudioData outClip))
        {
            Debug.LogError($"사운드가 딕셔너리에없음 : {clipName}");
            return null;
        }
        
        GameObject go = new GameObject($"AudioClip_{clipName}");
        go.transform.position = pos; // 넣어준 위치에 오디오 클립 재생을 위한 오디오 소스를 생성한다.
        AudioSource audioClip = go.AddComponent<AudioSource>();
        
        audioClip.clip = outClip.clipSource;
        audioClip.volume = outClip.volume;
        audioClip.loop = outClip.loop;
        audioClip.spatialBlend = 1; // 3D로 재생
        
        if (outClip.mixerGroup) // 믹서 그룹 설정
        {
            audioClip.outputAudioMixerGroup = outClip.mixerGroup;
        }
        
        audioClip.Play();

        if (!outClip.loop)
        {
            Destroy(go, outClip.clipSource.length); // 루프 재생이 아닐 경우, 길이가 끝나면 파괴
        }

        return audioClip;
    }
    
    public void SwitchBGM(string bgmName, float fadeTime = 1f) // 배경 음악 변경 시 사용
    {
        // 딕셔너리에서 이름을 기준으로 찾는다.
        if (!audioDict.TryGetValue(bgmName, out AudioData newBgm))
        {
            Debug.LogWarning($"브금 못찾음:{bgmName}");
            return;
        }
        
        // 이미 재생중이면 return;
        if (bgmSource.isPlaying && bgmSource.clip == newBgm.clipSource) return;
        
        // 재생중이 아니고, 클립도 비어 있으면 갈아끼우기
        if (!bgmSource.isPlaying && bgmSource.clip == null)
        {
            if (startBgmRoutine != null)
            {
                StopCoroutine(startBgmRoutine);
                startBgmRoutine = null;
            }
            startBgmRoutine = StartCoroutine(StartBGMRoutine(newBgm, fadeTime));
            return;
        }
        
        // fade 루틴이 진행중이였으면 중지
        if (bgmFadeRoutine != null)
        {
            StopCoroutine(bgmFadeRoutine);
            bgmFadeRoutine = null;
        }
        
        // 페이드 시작
        bgmFadeRoutine = StartCoroutine(FadeBGM(newBgm,fadeTime));

    }

    // BGM 페이드 인 코루틴
    private IEnumerator StartBGMRoutine(AudioData newBgm, float fadeTime)
    {
        // 클립 지정
        bgmSource.clip = newBgm.clipSource;
        bgmSource.volume = 0f;
        bgmSource.loop = newBgm.loop;
        bgmSource.Play();

        // 페이드 인
        float fadeTimer = 0f;
        while (fadeTimer < fadeTime)
        {
            fadeTimer += Time.deltaTime;
            bgmSource.volume = Mathf.Lerp(0f, newBgm.volume, fadeTimer / fadeTime);
            yield return null;
        }
        
        // 페이드 종료
        bgmSource.volume = newBgm.volume;
        StopCoroutine(startBgmRoutine);
        startBgmRoutine = null;
    }
    
    // BGM 페이드 아웃/인을 담당하는 코루틴
    private IEnumerator FadeBGM(AudioData newBgm, float fadeTime)
    {
        float fadeTimer = 0f;
        float startVolume = bgmSource.volume;
        
        // 페이드 아웃
        while (fadeTimer < fadeTime)
        {
            fadeTimer += Time.deltaTime;
            bgmSource.volume = Mathf.Lerp(startVolume, 0f, fadeTimer / fadeTime);
            yield return null;
        }

        bgmSource.Stop();
        bgmSource.clip = newBgm.clipSource;
        bgmSource.volume = 0f;
        bgmSource.loop = newBgm.loop;
        bgmSource.Play();
        
        // 페이드 인
        fadeTimer = 0f;
        while (fadeTimer < fadeTime)
        {
            fadeTimer += Time.deltaTime;
            bgmSource.volume = Mathf.Lerp(0f, newBgm.volume, fadeTimer / fadeTime);
            yield return null;
        }
        
        // 페이드 종료
        bgmSource.volume = newBgm.volume;
        StopCoroutine(bgmFadeRoutine);
        bgmFadeRoutine = null;
    }
    

    public void SwitchAmbient(string ambName,float fadeTime = 0.3f) // 백색 소음 변경 시 사용
    {
        if (!audioDict.TryGetValue(ambName, out AudioData newAmb))
        {
            Debug.LogWarning($"엠비언트 찾지 못함 : {ambName}");
            return;
        }
        
        // 플레이 중인데 같은 엠비언트 재생 중
        if (ambientSource.isPlaying && ambientSource.clip == newAmb.clipSource) return;

        if (!ambientSource.isPlaying && ambientSource.clip == null)
        {
            if (startAmbRoutine != null)
            {
                StopCoroutine(startAmbRoutine);
                startAmbRoutine = null;
            }

            startAmbRoutine = StartCoroutine(StartAmbientRoutine(newAmb, fadeTime));
        }
        // fade 루틴이 진행중이였으면 중지
        if (ambFadeRoutine != null)
        {
            StopCoroutine(ambFadeRoutine);
            ambFadeRoutine = null;
        }
        
        // 페이드 시작
        ambFadeRoutine = StartCoroutine(FadeAmbient(newAmb,fadeTime));
    }

    // 재생 중이지 않을 때 페이드 인 시작
    private IEnumerator StartAmbientRoutine(AudioData newAmb, float fadeTime)
    {
        ambientSource.clip = newAmb.clipSource;
        ambientSource.volume = 0f;
        ambientSource.loop = newAmb.loop;
        ambientSource.Play();
        
        // 페이드 인
        float fadeTimer = 0f;
        while (fadeTimer < fadeTime)
        {
            fadeTimer += Time.deltaTime;
            ambientSource.volume = Mathf.Lerp(0f, newAmb.volume, fadeTimer / fadeTime);
            yield return null;
        }
        
        // 페이드 종료
        ambientSource.volume = newAmb.volume;
        StopCoroutine(startAmbRoutine);
        startAmbRoutine = null;
    }
    
    // 페이드 아웃/인 변경
    private IEnumerator FadeAmbient(AudioData newAmbient, float fadeTime)
    {
        float fadeTimer = 0f;
        float startVolume = ambientSource.volume;
        
        // 페이드 아웃
        while (fadeTimer < fadeTime)
        {
            fadeTimer += Time.deltaTime;
            ambientSource.volume = Mathf.Lerp(startVolume, 0f, fadeTimer / fadeTime);
            yield return null;
        }

        ambientSource.Stop();
        ambientSource.clip = newAmbient.clipSource;
        ambientSource.volume = 0f;
        ambientSource.loop = newAmbient.loop;
        ambientSource.Play();
        
        // 페이드 인
        fadeTimer = 0f;
        while (fadeTimer < fadeTime)
        {
            fadeTimer += Time.deltaTime;
            ambientSource.volume = Mathf.Lerp(0f, newAmbient.volume, fadeTimer / fadeTime);
            yield return null;
        }
        
        // 페이드 종료
        ambientSource.volume = newAmbient.volume;
        StopCoroutine(ambFadeRoutine);
        ambFadeRoutine = null;
    }
    
    public void PlayEffect(string effectName) // 공간감 없는 이펙트음 재생에 쓰인다. UI 등에 쓰임
    {
        if (!audioDict.TryGetValue(effectName, out AudioData outEffect))
        {
            Debug.LogError($"사운드 파일이 없음 : {effectName}");
            return;
        }
        effectSource.clip = outEffect.clipSource;
        effectSource.volume = outEffect.volume;
        effectSource.loop = outEffect.loop;

        if (outEffect.mixerGroup)
        {
            effectSource.outputAudioMixerGroup = outEffect.mixerGroup;
        }
        effectSource.Play();
    }

    public void SetAudioVolume(MixerType type, float volume) // 마스터 볼륨 설정. 0~1 사이의 값이 들어온다.
    {
        switch (type)
        {
            case MixerType.Master:
                mixer.SetFloat("MasterVolume", Mathf.Log10(Mathf.Clamp(volume,0.0001f,1f))*20);
                PlayerPrefs.SetFloat("MasterVolume", volume);
                break;
            case MixerType.BGM:
                mixer.SetFloat("BGMVolume", Mathf.Log10(Mathf.Clamp(volume,0.0001f,1f))*20);
                PlayerPrefs.SetFloat("BGMVolume", volume);
                break;
            case MixerType.SFX:
                mixer.SetFloat("SFXVolume", Mathf.Log10(Mathf.Clamp(volume,0.0001f,1f))*20);
                PlayerPrefs.SetFloat("SFXVolume", volume);
                break;
            default:
                Debug.LogWarning($"믹서 타입이 잘못들어옴{type}");
                return;
        }
        
    }

    public void StopAllSounds()
    {
        if (stopRoutine != null)
        {
            StopCoroutine(stopRoutine);
            stopRoutine = null;
        }
        stopRoutine = StartCoroutine(StopAllSoundRoutine());
    }

    private IEnumerator StopAllSoundRoutine()
    {
        float fadeTimer = 0f;
        float fadeTime = 1f;
        float startVolume = bgmSource.volume; 
        while (fadeTimer < fadeTime)
        {
            fadeTimer += Time.deltaTime;

            bgmSource.volume = Mathf.Lerp(startVolume, 0f, fadeTimer / fadeTime);
            ambientSource.volume = Mathf.Lerp(startVolume, 0f, fadeTimer / fadeTime);
            yield return null;
        }

        bgmSource.Stop();
        ambientSource.Stop();
        bgmSource.clip = null;
        ambientSource.clip = null;
    }

    public void SetFireSound(BaseController player)
    {
        if (!audioDict.TryGetValue("Fire", out AudioData outFire))
        {
            Debug.LogWarning("Fire 오디오 소스가 없음");
            return;
        }

        GameObject go = new GameObject("FireSound");
        go.transform.position = player.transform.position;
        AudioSource audio = go.AddComponent<AudioSource>();
        
        audio.clip = outFire.clipSource;
        audio.volume = outFire.volume;
        audio.loop = true; // 루프 켜줌
        audio.playOnAwake = false; // 생기자마자 재생 X
        audio.spatialBlend = 1f; // 3D효과
        audio.transform.SetParent(player.transform); // 부모설정
        if (outFire.mixerGroup) // 믹서 그룹 설정
        {
            audio.outputAudioMixerGroup = outFire.mixerGroup;
        }
        
        player.fireSound = audio;
    }

    public enum MixerType{Master,BGM,SFX}
}
