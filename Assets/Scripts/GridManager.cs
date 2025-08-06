using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class GridManager : MonoBehaviour, IInRoomCallbacks
{
    [SerializeField] private TMP_Text teamRateText;
    [SerializeField] private GameObject MapWithGrid;
    private PhotonView photonView;///

    private int countTeam1  = 0;
    private int countTeam2 = 0;
    private int countNone = 0;

    public float Team1Rate { get; private set; }
    public float Team2Rate { get; private set; }

    public Dictionary<int, MapGrid> gridDic = new(5000);
    private Dictionary<Collider, MapGrid> colliderToGrid = new(5000);
    //그리드들을 넣어놓을 딕셔너리

    private void Awake()
    {
        photonView = GetComponent<PhotonView>();//
        Manager.Grid = this;

        MapGrid[] mapGrids = MapWithGrid.GetComponentsInChildren<MapGrid>(false);
        for (int i = 0; i < mapGrids.Length; i++)
        {
            mapGrids[i].id = i;
        }   

    }
    private void Start()
    {
        Debug.Log("그리드매니저 스타트");
    }

    private void OnEnable()
    {
        PhotonNetwork.AddCallbackTarget(this);
    }

    private void OnDisable()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }

    public void RegisterGrid(MapGrid grid)
    {
        int id = grid.id;
        if(!gridDic.ContainsKey(id))
            // 딕셔너리에 없다면
        {
            gridDic.Add(id, grid);
            //딕셔너리에 등록
            countNone++;
            //그리드를 등록할때는 팀을none 으로 넣음

            Collider col = grid.GetComponent<Collider>();
            if (col != null)
            {
                colliderToGrid[col] = grid;
            }
        }
    }
    public MapGrid GetGrid(Collider col) //콜라이더로 그리드 정보 받아옴
    {
        colliderToGrid.TryGetValue(col, out MapGrid grid);
        return grid;
    }

    //public MapGrid GetGrid(GameObject obj) //
    //{
    //    Collider col = obj.GetComponent<Collider>();
    //    if (col != null)
    //        return GetGrid(col);
    //    else
    //        return null;
    //}

    public MapGrid GetGridByID(int id)
    {
        gridDic.TryGetValue(id, out MapGrid grid);
        return grid;
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
        Team1Rate = countTeam1 / (float)total * 100f;
        Team2Rate = countTeam2 / (float)total * 100f;
        teamRateText.text = $"Team1 : {Team1Rate.ToString("F2")}%    Team2 : {Team2Rate.ToString("F2")}%";
        //teamRateText.text = "";

        //photonView.RPC("SyncCoverageUI", RpcTarget.Others, Team1Rate, Team2Rate);//
   
    }


    //[PunRPC]
    //public void SyncCoverageUI(float team1Rate, float team2Rate) 
    //{
    //    teamRateText.text = $"Team1 : {team1Rate:F2}%    Team2 : {team2Rate:F2}%";
    //    //teamRateText.text = ""; 
    //}

    public string GetWinningTeam()
    {
        if (countTeam1 > countTeam2) return "Purple";
        else if (countTeam2 > countTeam1) return "Yellow";
        else return "Draw";
    }


   public void OnMasterClientSwitched(Player newMasterClient)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            CountGridsByNewMaster();
            Debug.Log("마스터 바뀜");
        }
    }

    private void CountGridsByNewMaster()
    {
        countTeam1 = 0;
        countTeam2 = 0;
        countNone = 0;

        foreach (MapGrid grid in gridDic.Values)
        {

            if(grid.team == Team.Team1)
            {
                countTeam1++;
            }

            else if(grid.team == Team.Team2)
            {
                countTeam2++;
            }

            else if(grid.team == Team.None)
            {
                countNone++;
            }

        }
        UpdateUI();
    }


    // 인터페이스때문에 필요하지만 안씀
    public void OnPlayerEnteredRoom(Player newPlayer) { }
    public void OnPlayerLeftRoom(Player otherPlayer) { }
    public void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged) { }
    public void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps) { }
}

