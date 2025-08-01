using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class GridManager : Singleton<GridManager>
{
    [SerializeField] private TMP_Text teamRateText;
    private PhotonView photonView;///

    private int countTeam1  = 0;
    private int countTeam2 = 0;
    private int countNone = 0;

    public float Team1Rate { get; private set; }
    public float Team2Rate { get; private set; }

    private Dictionary<int, MapGrid> gridDic = new(5000);
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

        photonView = GetComponent<PhotonView>();//

    }
    private void Start()
    {
        Debug.Log("그리드매니저 스타트");
    }

    public void RegisterGrid(MapGrid grid)
    {
        int id = grid.gameObject.GetInstanceID();
        if(!gridDic.ContainsKey(id))
            // 딕셔너리에 없다면
        {
            gridDic.Add(id, grid);
            //딕셔너리에 등록
            countNone++;
            //그리드를 등록할때는 팀을none 으로 넣음
        }
    }
    public MapGrid GetGrid(GameObject obj)
    {
        int id = obj.GetInstanceID();
        return gridDic[id];
        
    }    

    public List<MapGrid> GetAllGrids() //InkParticleCollision 에서 한번 호출
    {
        //Debug.Log($"등록된 그리드 총개수: {gridDic.Count}");
        //Debug.Log($"None :{countNone}");
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
        Team1Rate = countTeam1 / (float)total * 100f;
        Team2Rate = countTeam2 / (float)total * 100f;
        teamRateText.text = $"Team1 : {Team1Rate.ToString("F2")}%    Team2 : {Team2Rate.ToString("F2")}%";

        photonView.RPC("SyncCoverageUI", RpcTarget.Others, Team1Rate, Team2Rate);
    }

    [PunRPC]
    public void SyncCoverageUI(float team1Rate, float team2Rate) // 추가됨!!!
    {
        teamRateText.text = $"Team1 : {team1Rate:F2}%    Team2 : {team2Rate:F2}%";
    }

    public string GetWinningTeam()
    {
        if (countTeam1 > countTeam2) return "Team1";
        else if (countTeam2 > countTeam1) return "Team2";
        else return "Draw";
    }
}
