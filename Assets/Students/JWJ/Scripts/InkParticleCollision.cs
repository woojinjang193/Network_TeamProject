using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InkParticleCollision : MonoBehaviour
{
    private TeamColorInfo teamColorInfo;  //팀컬러 정보
    private Team myTeam; //팀 정보
    private ParticleSystem particleSys; // 충돌이벤트를 위한 파티클시스템 변수
    List<ParticleCollisionEvent> events = new();  //파티클 충돌 이벤트, 파티클 충돌 이벤트는 리스트로 넣어야함
    List<ParticleSystem.Particle> enter = new (); //파티클 트리거 충돌 이벤트 리스트 

    [SerializeField] private float radius;  //반지름
    [SerializeField] private float hardness; // 원 선명도
    [SerializeField] private float strength; // 강도?

    private void Awake()
    {
        teamColorInfo = FindObjectOfType<TeamColorInfo>();
        //팀컬러 정보를 가져옴
        particleSys = GetComponent<ParticleSystem>();
        //파티클 시스템을 가져옴
    }

    private void Start()
    {
        ParticleSystem.TriggerModule triggerModule = particleSys.trigger;
        //트리거모듈 설정을 위해 triggerModule 추가

        foreach (Grid grid in Manager.Grid.GetAllGrids())
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

            if(hitComponent.gameObject.TryGetComponent<PaintableObj>(out PaintableObj paintableObj))
                //페인트칠 가능 오브젝트라면
            {
                paintableObj.DrawInk(hitPos, radius, hardness, strength, myTeam);
                //페인트 칠함
            }

            if(hitComponent.CompareTag("Player"))
                //플레이어라면
            {
                HitPlayer(hitComponent.gameObject);
                //팀판정 및 후처리
            }
        }
    }

    private void OnParticleTrigger()
    {
        int numEnter = particleSys.GetTriggerParticles(ParticleSystemTriggerEventType.Enter, enter);
        //트리거Enter 한 파티클 수

        for (int i = 0; i < numEnter; i++)  
            //충돌한 파티클 수 만큼 반복
        {
            Vector3 hitPos = enter[i].position;
            //충돌 위치

            Collider[] colliders = Physics.OverlapSphere(hitPos, 0.1f);
            //충돌 위치 주변 콜라이더 탐색

            foreach (Collider collider in colliders)
                //콜라이더 리스트 체크
            {
                if (collider.CompareTag("Grid"))
                    //태그가 Grid 라면
                {
                    Grid grid = collider.GetComponent<Grid>();
                    grid.SetGrid(myTeam);
                    //팀 정보 넣어줌
                }
            }
        }
    }

    private void HitPlayer(GameObject gameObj)
    {
        gameObj.TryGetComponent<PlayerTestController>(out PlayerTestController player);
        //플레이어 팀정보 가진 컨포넌트 가져옴 (추후에 변경해야함)

        if (player == null)
        {
            Debug.Log("플레이어컨트롤러 컨포넌트 없음");
            return;
        }
        else
        {
            if(player.MyTeam == myTeam)
            {
                Debug.Log("아군입니다.");
                return;
            }
            if (player.MyTeam != myTeam)
            {
                Debug.Log("적 입니다.");
                //데미지전송 구현해야함

            }
            else
            {
                Debug.Log("팀 이없습니다");
            }
        }

    }
}
