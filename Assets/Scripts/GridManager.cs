using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class GridManager : Singleton<GridManager>
{
    [SerializeField] private TMP_Text teamRateText;
    public static GridManager GetInstance() => Instance;

    private int countTeam1 = 0;
    private int countTeam2 = 0;
    private int countNone = 0;

    private Dictionary<string, MapGrid> gridDic = new(5000);
    //그리드들을 넣어놓을 딕셔너리

    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(gameObject);
    }
    private void Start()
    {
        Debug.Log("그리드매니저 스타트");
    }

    public void RegisterGrid(MapGrid grid)
    {
        if(!gridDic.ContainsKey(grid.gameObject.name))
            // 딕셔너리에 없다면
        {
            gridDic.Add(grid.gameObject.name, grid);
            //딕셔너리에 등록
            countNone++;
            //그리드를 등록할때는 팀을none 으로 넣음
        }
    }
    public MapGrid GetGrid(GameObject obj)
    {
        return gridDic[obj.name];
    }    

    public List<MapGrid> GetAllGrids()
    {
        return gridDic.Values.ToList();
        //딕셔너리를 리스트로 바꿔서 반환
    }

    public void UpdateCoverageRate()
    {
        int total = gridDic.Count;
        int team1 = 0;
        int team2 = 0;
        int none = 0;

        foreach (MapGrid grid in gridDic.Values)  //그리드에서 이벤트로 ++ 넘겨줌 수정 해야함
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
