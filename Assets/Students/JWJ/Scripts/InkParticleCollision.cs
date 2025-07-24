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
    [SerializeField] private float hardness; // 강도
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
        if (!other.gameObject.TryGetComponent<PaintableObj>(out PaintableObj paintableObj))
        //충돌체가 PaintableObj 컴포넌트를 가지고있지 않다면
        {
            return; //리턴
        }

        events.Clear(); //이벤트 실행전 초기화

        int count = particleSys.GetCollisionEvents(other, events);
        //충돌한 파티클 수를 count에 넣어줌
        for (int i = 0; i < count; i++) //충돌한 파티클 수만큼 반복
        {
            Vector3 hitPos = events[i].intersection;
            // i번째 충돌 이벤트의 월드좌표기준 위치를 담아줌
            paintableObj.DrawInk(hitPos, radius, hardness, strength, myTeam);
            //페인트 그려주는 DrawInk에 매개변수를 담아 보냄
            //Debug.Log($"페인트 가능 오브젝트 : {paintableObj.name}");
        }

        if (other.gameObject.TryGetComponent<PlayerTestController>(out PlayerTestController player))
            //충돌체가 플레이어일경우
        {
            if (player.MyTeam != myTeam)
                //아군이 아닐경우
            {
                Debug.Log("적에게 명중");
            }
            else
            //아군일 경우 
            {
                Debug.Log("아군 에게 명중");
            }
        }
    }
}
