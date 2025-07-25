using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InkParticleCollision : MonoBehaviour
{
    private TeamColorInfo teamColorInfo;  //팀컬러 정보
    private Team myTeam; //팀 정보
    private ParticleSystem particleSys; // 충돌이벤트를 위한 파티클시스템 변수
    List<ParticleCollisionEvent> events = new();  //파티클 충돌 이벤트, 파티클 충돌 이벤트는 리스트로 넣어야함

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

    public void SetTeam(Team team)
    {
        myTeam = team;
        //InkParticleGun 에서 팀정보를 넘겨받음
    }

    private void OnParticleCollision(GameObject other)
    {
        events.Clear(); //이벤트 실행전 초기화
        int count = particleSys.GetCollisionEvents(other, events);

        for (int i = 0; i < count; i++)
        {
            Vector3 hitPos = events[i].intersection;
            var hitComponent = events[i].colliderComponent;

            if(hitComponent.gameObject.TryGetComponent<PaintableObj>(out PaintableObj paintableObj))
            {
                paintableObj.DrawInk(hitPos, radius, hardness, strength, myTeam);
            }

            if(hitComponent.CompareTag("Player"))
            {
                HitPlayer(hitComponent.gameObject);
            }
        }
    }

    private void HitPlayer(GameObject gameObj)
    {
        gameObj.TryGetComponent<PlayerTestController>(out PlayerTestController player);

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
                //데미지 전송
            }
            else
            {
                Debug.Log("팀 이없습니다");
            }
        }

    }
}
