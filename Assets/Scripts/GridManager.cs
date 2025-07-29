using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class GridManager : Singleton<GridManager>
{
    [SerializeField] private TMP_Text teamRateText;

    private int countTeam1 = 0;
    private int countTeam2 = 0;
    private int countNone = 0;

    private Dictionary<string, MapGrid> gridDic = new(5000);
    //그리드들을 넣어놓을 딕셔너리

    protected override void Awake()
    {
        base.Awake();

        if(teamRateText == null)
        {
            GameObject teamRateTextPrefab = Resources.Load<GameObject>("UI/CoverageRateCanvas");
            if (teamRateTextPrefab != null)
            {
                GameObject canvas = Instantiate(teamRateTextPrefab);
                canvas.transform.SetParent(transform, false);
                teamRateText = canvas.GetComponentInChildren<TMP_Text>();
            }
            else
            {
                Debug.LogError("UI/CoverageRateCanvas 프리팹을 찾을 수 없습니다.");
            }
        }
     
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

    public List<MapGrid> GetAllGrids() //InkParticleCollision 에서 한번 호출
    {
        return gridDic.Values.ToList();
        //딕셔너리를 리스트로 바꿔서 반환
    }

    public void ChangeGridTeam(Team oldTeam, Team newTeam)
    {
        if(oldTeam == Team.Team1)
        {
            countTeam1--;
        }
        else if (oldTeam == Team.Team2)
        {
            countTeam2--;
        }
        else
        {
            countNone--;
            //처음 업데이트시
        }

        if (newTeam == Team.Team1)
        {
            countTeam1++;
        }
        else if (newTeam == Team.Team2)
        {
            countTeam2++;
        }
        else
        {
            countNone++;
            //처음 업데이트시
        }

        UpdateUI();
    }

    private void UpdateUI()
    {
        int total = gridDic.Count;
        float Team1Rate = countTeam1 / (float)total * 100f;
        float Team2Rate = countTeam2 / (float)total * 100f;
        teamRateText.text = $"Team1 : {Team1Rate.ToString("F2")}%    Team2 : {Team2Rate.ToString("F2")}%";
    }
}
