using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeaderBoard : MonoBehaviour
{

    [SerializeField] public List<LeaderBoardEntry> ranker = new List<LeaderBoardEntry>();

    
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            Debug.Log("R키 누른것은 인식 됨");
            if (FirebaseManager.LeaderBoard == null)
            {
                FirebaseManager.LeaderBoard = this;
            }
            // Manager.FB.GetLeaderboard();
        }
    }
    
    public void ListUp(string nickName, int win, float winRate)
    {
        LeaderBoardEntry entry = new LeaderBoardEntry(nickName,win,winRate);
        ranker.Add(entry);
    }
}
[System.Serializable]
public class LeaderBoardEntry
{
    public string nickName;
    public int win;
    public float winRate;

    public LeaderBoardEntry(string nickName, int win, float winRate)
    {
        this.nickName = nickName;
        this.win = win;
        this.winRate = winRate;
    }
}

