using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InkParticleCollision : MonoBehaviour //파티클 충돌을 관리하는 클래스
{
    private PhotonView photonView;
    private TeamColorInfo teamColorInfo;  //팀컬러 정보
    private Team myTeam; //팀 정보
    private ParticleSystem particleSys; // 충돌이벤트를 위한 파티클시스템 변수
    List<ParticleCollisionEvent> events = new();  //파티클 충돌 이벤트, 파티클 충돌 이벤트는 리스트로 넣어야함
    List<ParticleSystem.Particle> enter = new (); //파티클 트리거 충돌 이벤트 리스트 

    [SerializeField] private float radius;  //반지름
    [SerializeField] private float hardness; // 원 선명도
    [SerializeField] private float strength; // 강도

    private Collider[] colliders = new Collider[10];
    //충돌때마다 배열을 생성하지 않기위해 미리 생성

    private Dictionary<Collider, PaintableObj> paintableObject;
    //Collider를 키로, PaintableObj를 값으로 딕셔너리를 만들변수

    private void Awake()
    {
        photonView = GetComponent<PhotonView>();
        //포톤뷰

        teamColorInfo = FindObjectOfType<TeamColorInfo>();
        //팀컬러 정보를 가져옴
        particleSys = GetComponent<ParticleSystem>();
        //파티클 시스템을 가져옴

        paintableObject = new Dictionary<Collider, PaintableObj> ();
        //캐싱용 딕셔너리 생성

        PaintableObj[] paintableObjs = FindObjectsOfType<PaintableObj>();
        // 모든 PaintableObj를 넣을 배열 생성 

        //배열을 돌면서 PaintableObj 컴포넌트를 가진 객체 안에 콜라이더(딕셔너리 키)를 찾음
        for (int i = 0; i < paintableObjs.Length; i++)
        {
            PaintableObj paintableObj = paintableObjs[i];
            if(paintableObj.TryGetComponent<Collider>(out Collider collider))
            {
                paintableObject[collider] = paintableObj;
                //딕셔너리에 키(collider) 와 벨류(paintableObj)를 저장
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
        events.Clear(); //이벤트 실행전 초기화
        int count = particleSys.GetCollisionEvents(other, events);
        //충돌한 파티클 수

        for (int i = 0; i < count; i++)
        {
            Vector3 hitPos = events[i].intersection; //충돌 위치정보
            var hitComponent = events[i].colliderComponent; // 충돌한 컴포넌트
            Collider collider = hitComponent as Collider;
            //hitComponent 타입을 Collider 으로 변경 시도

            if (collider == null)
            {
                continue;
            }
            else
            {
                if (paintableObject.TryGetValue(collider, out PaintableObj paintableObj))
                //타입 변환에 성공 && 딕셔너리에 키를 넣어 값을 받음
                {
                    paintableObj.DrawInk(hitPos, radius, hardness, strength, myTeam);
                    //페인트 칠함
                }

                PlayerController player = Manager.Game.GetPlayer(collider);
                if (player != null)
                {
                    if(photonView.IsMine)
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
        if(!PhotonNetwork.IsMasterClient) //마스터 클라이언트만 실행
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
                if(collider.CompareTag("Grid"))
                    //태그가 Grid라면
                {
                    Manager.Grid.GetGrid(collider.gameObject).SetGrid(myTeam);
                    //그리드 정보를 가져와서 팀을 세팅해줌
                }
            }
        }
    }

    private void HitPlayer(PlayerController player)
    {
        if (player.MyTeam == myTeam) 
        {
            Debug.Log("아군입니다.");
            return;
        }
        else if (player.MyTeam != myTeam)
        {
            Debug.Log("적 입니다.");
            player.photonView.RPC("TakeDamage", player.photonView.Owner, 0.2f);
        }
        else
        {
            Debug.Log("팀 이없습니다");
        }
    }
}
