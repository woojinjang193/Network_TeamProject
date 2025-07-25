using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GridManager : Singleton<GridManager>
{
    [SerializeField] private TMP_Text teamRateText;
    public static GridManager GetInstance() => Instance;

    private List<Grid> grids = new();
    //그리드들을 넣어놓을 리스트

    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(gameObject);
    }
    private void Start()
    {
        Debug.Log("그리드매니저 스타트");
    }

    public void RegisterGrid(Grid grid)
    {
        if (!grids.Contains(grid))
            //이미 등록된 그리드가 아닐때
        {
            grids.Add(grid);
            //리스트에 추가
        }
    }
    public List<Grid> GetAllGrids()
    {
        return grids;
    }

    public void UpdateCoverageRate()
    {
        int total = grids.Count;
        int team1 = 0;
        int team2 = 0;
        int none = 0;

        foreach (Grid grid in grids)
        {
            if (grid.team == Team.Team1)
            {
                team1++;
            }
            else if (grid.team == Team.Team2)
            {
                team2++;
            }
            else
            {
                none++;
            }
        }

        float team1Rate = team1 / (float)total * 100f;
        float team2Rate = team2 / (float)total * 100f;
        float NoneRate = none / (float)total * 100f;

        teamRateText.text = $"Team1 : {(int)team1Rate}%    Team2 : {(int)team2Rate}%";
        //Debug.Log($"팀1 : {team1Rate}, 팀2 : {team2Rate}");
    }


}
