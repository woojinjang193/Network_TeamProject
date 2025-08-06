using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InkParticleCollision : MonoBehaviourPun //파티클 충돌을 관리하는 클래스
{
    //private PhotonView photonV;
    //private TeamColorInfo teamColorInfo;  //팀컬러 정보
    private Team myTeam; //팀 정보
    private ParticleSystem particleSys; // 충돌이벤트를 위한 파티클시스템 변수
    List<ParticleCollisionEvent> events = new();  //파티클 충돌 이벤트, 파티클 충돌 이벤트는 리스트로 넣어야함
    List<ParticleSystem.Particle> enter = new(); //파티클 트리거 충돌 이벤트 리스트 

    [Header("Set References")] 
    [SerializeField] private ParticleSystem splash;
    
    [Header("Set Values")]
    [SerializeField] private float radius;  //반지름
    [SerializeField] private float hardness; // 원 선명도
    [SerializeField] private float strength; // 강도

    private Collider[] colliders = new Collider[10];
    //충돌때마다 배열을 생성하지 않기위해 미리 생성 (트리거용)

    private Dictionary<Collider, PaintableObj> dicColliderToPaintable = new();
    //Collider를 키로, PaintableObj를 값으로 딕셔너리 생성
    private Dictionary<PaintableObj, int> dicPaintableToViewID = new();
    //PaintableObj를 키로, int를 값으로 딕셔너리 생성
    private Dictionary<int, PaintableObj> dicViewIDToPaintable = new();
    //Collider를 키로, PaintableObj를 값으로 딕셔너리 생성

    private void Awake()
    {
        //photonV = GetComponent<PhotonView>();
        //포톤뷰

        //teamColorInfo = FindObjectOfType<TeamColorInfo>();
        //팀컬러 정보를 가져옴
        particleSys = GetComponent<ParticleSystem>();
        //파티클 시스템을 가져옴

        PaintableObj[] paintableObjs = FindObjectsOfType<PaintableObj>();
        // 모든 PaintableObj를 넣을 배열 생성 

        //배열을 돌면서 PaintableObj 컴포넌트를 가진 객체 안에 콜라이더, 포톤뷰(딕셔너리 키)를 찾음
        for (int i = 0; i < paintableObjs.Length; i++)
        {
            PaintableObj paintableObj = paintableObjs[i];
            if (paintableObj.TryGetComponent<Collider>(out Collider collider) && paintableObj.TryGetComponent<PhotonView>(out PhotonView pv))
            //콜라이더와 포톤뷰 컴포넌트를 가져옴
            {
                int viewID = pv.ViewID;

                //딕셔너리에 등록
                dicColliderToPaintable[collider] = paintableObjs[i];
                dicPaintableToViewID[paintableObjs[i]] = viewID;
                dicViewIDToPaintable[viewID] = paintableObjs[i];
            }
        }
    }

    private void Start()
    {
        ParticleSystem.TriggerModule triggerModule = particleSys.trigger;
        //트리거모듈 설정을 위해 triggerModule 추가

        foreach (MapGrid grid in Manager.Grid.GetAllGrids())
        //매니저에서 모든 그리드 받아옴
        {
            Collider collider = grid.GetComponent<Collider>();
            //그리드의 Collider 컴포넌트 가져옴
            if (collider != null)
            {
                int index = triggerModule.colliderCount;
                //foreach가 돌면서 콜라이더를 가져오면 colliderCount가 올라가고 index가 같이 증가함
                triggerModule.SetCollider(index, collider);
                //그리드 등록
            }
        }
    }

    public void SetTeam(Team team)
    {
        myTeam = team;
        //InkParticleGun 에서 팀정보를 넘겨받음
    }

    private void OnParticleCollision(GameObject other)
    {
        if (photonView.IsMine)
        {
            events.Clear(); //이벤트 실행전 초기화
            int count = particleSys.GetCollisionEvents(other, events);
            //충돌한 파티클 수

            for (int i = 0; i < count; i++)
            {
                Vector3 hitPos = events[i].intersection; //충돌 위치정보
                var hitComponent = events[i].colliderComponent; // 충돌한 컴포넌트
                Collider collider = hitComponent as Collider;
                //hitComponent 타입을 Collider 으로 변경 시도

                Vector3 hitNor = events[i].normal; // 충돌 지점 포워드

                if (collider == null)
                {
                    continue;
                }
                
                if (dicColliderToPaintable.TryGetValue(collider, out PaintableObj paintableObj))
                    //타입 변환에 성공 && 딕셔너리에 키를 넣어 값을 받음
                {
                    if (dicPaintableToViewID.TryGetValue(paintableObj, out int viewID))
                        //위에서 받은 값으로 칠해질 오브젝트 viewID 가져옴
                    {
                        photonView.RPC("ReportPaint", RpcTarget.MasterClient, hitPos, radius, hardness, strength, (int)myTeam, viewID, hitNor);
                        //뷰아이디를 포함해서 마스터 클라이언트한테 보고
                    }

                }

                BaseController player = Manager.Game.GetPlayer(collider);
                //collider 를 키로 플레이어를 받아옴
                if (player != null)
                {
                    if (photonView.IsMine)
                    {
                        HitPlayer(player);
                        //팀판정 및 후처리
                    }

                }
            } 
        }
    }

    private void OnParticleTrigger()
    {
        if (!PhotonNetwork.IsMasterClient) //마스터 클라이언트만 실행
        {
            //Debug.Log("마스터 클라이언트가 아닙니다.");
            return;
        }
        //Debug.Log("트리거 충돌.");

        int numEnter = particleSys.GetTriggerParticles(ParticleSystemTriggerEventType.Enter, enter);
        //트리거Enter 한 파티클 수

        for (int i = 0; i < numEnter; i++)
        //충돌한 파티클 수 만큼 반복
        {
            Vector3 hitPos = enter[i].position;
            //Debug.Log($"Pos {hitPos.x}, {hitPos.y}, {hitPos.z}");
            //충돌 위치

            int hitCount = Physics.OverlapSphereNonAlloc(hitPos, 0.5f, colliders);
            //충돌 위치 주변 콜라이더 탐색하여 수를 넣어줌

            for (int j = 0; j < hitCount; j++)
            {
                Collider collider = colliders[j];
                //배열의 콜라이더를 하나씩 검사
                if (collider.CompareTag("Grid"))
                //태그가 Grid라면
                {
                    MapGrid grid = Manager.Grid.GetGrid(collider);
                    //콜라이더로 그리드 정보 받아옴
                    if (grid == null)
                    {
                        continue;
                    }
                        
                    int gridID = grid.id; //그디드 아이디
                    int teamIndex = (int)myTeam; //팀정보 RPC를 위해 int로 변경
                    photonView.RPC("RpcSetGridTeam", RpcTarget.All, gridID, teamIndex);
                }
            }
        }
    }

    [PunRPC]
    private void RpcSetGridTeam(int gridID, int teamIndex) // 모든클라이언트가 그리드 팀정보 업데이트
    {
        var grid = Manager.Grid.GetGridByID(gridID);
        //Debug.Log(grid);

        if (grid != null)
        {
            grid.SetGrid((Team)teamIndex);
        }
    }

    [PunRPC]
    private void ReportPaint(Vector3 hitPos, float radius, float hardness, float strength, int teamIndex, int viewID, Vector3 hitNor)
    {
        photonView.RPC("SyncDrawInk", RpcTarget.All, hitPos, radius, hardness, strength, teamIndex, viewID, hitNor);
        //마스터가 넘겨받은 정보로 모든 플레이어가 같은곳에 칠하게 해줌
    }

    [PunRPC]
    private void SyncDrawInk(Vector3 hitPos, float radius, float hardness, float strength, int teamIndex, int viewID, Vector3 hitNor)
    {
        Team team = (Team)teamIndex;
        //인덱스로 enum 변환

        if (dicViewIDToPaintable.TryGetValue(viewID, out PaintableObj paintableObj))
        //뷰아이디로 칠해질 오브젝트 받아옴
        {
            paintableObj.DrawInk(hitPos, radius, hardness, strength, team);
            //그림
        }

        // 충돌 지점 스플래쉬 이펙트
        Instantiate(splash, hitPos, Quaternion.LookRotation(hitNor));
        
        // 충돌 지점에 소리 재생
        Manager.Audio.PlayClip("InkHit",hitPos);
        Manager.Audio.PlayClip("Splash",hitPos);
        Manager.Audio.PlayClip("Spread",hitPos);
    }


    private void HitPlayer(BaseController player)
    {
        if (player.MyTeam == myTeam)
        {
            Debug.Log("아군입니다.");
            return;
        }
        if (player.MyTeam != myTeam)
        {
            Debug.Log("적 입니다.");
            player.photonView.RPC("TakeDamage", player.photonView.Owner, 15f);
            Manager.Audio.PlayClip("InkHit",player.transform.position);
            Manager.Audio.PlayEffect("HitPlayer");
            return;
        }
        Debug.Log("팀 이없습니다");
    }
}
