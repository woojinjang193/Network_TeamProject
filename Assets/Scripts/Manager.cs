using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Manager : Singleton<Manager>
{
    public static GameManager Game;
    public static AudioManager Audio;
    public static GridManager Grid;

    protected override void Awake()
    {
        base.Awake();
        Game = GameManager.Instance;
        Audio = AudioManager.Instance;
        Grid = GridManager.Instance;
    }

}
