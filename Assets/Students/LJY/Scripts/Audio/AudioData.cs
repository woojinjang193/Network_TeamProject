using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

[System.Serializable]
[CreateAssetMenu(fileName = "NewSoundData", menuName = "Audio/Data") ]
public class AudioData : ScriptableObject
{
    public string clipName;
    public AudioClip clipSource;
    [Range(0f,1f)] public float volume = 1.0f;
    public bool loop = false;
    public AudioMixerGroup mixerGroup;
}
