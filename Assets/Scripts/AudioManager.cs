using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : Singleton<AudioManager>
{
    public static AudioManager GetInstance() => Instance;
    private void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(gameObject);
    }
    private void Start()
    {
        Debug.Log("오디오매니저 스타트");
    }

    public void PlayBGM()
    {
        Debug.Log("Play BGM");
    }
    public void StopBGM()
    {
        Debug.Log("Stop BGM");
    }

    public void PlaySFX ()
    {
        Debug.Log("Play SFX");
    }
    
}
