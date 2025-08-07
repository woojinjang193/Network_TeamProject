using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class UserData
{
    public string userName;
    public int wins;
    public int losses;
    public float winRate;

    public UserData(string userName, int wins, int losses, float winRate)
    {
        this.userName = userName;
        this.wins = wins;
        this.losses = losses;
        this.winRate = winRate;
    }
}
