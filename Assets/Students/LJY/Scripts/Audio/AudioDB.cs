using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewAudioDataBase",menuName = "Audio/DataBase")]
public class AudioDataBase : ScriptableObject
{
    public List<AudioData> audioList;
}
