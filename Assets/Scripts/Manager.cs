using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Manager
{
    public static GameManager Game => GameManager.GetInstance();
    public static AudioManager Audio => AudioManager.GetInstance();

}
