using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InkParticleCollision : MonoBehaviour
{
    private TeamColorInfo teamColorInfo;
    private Team myTeam;
    private ParticleSystem particleSys;
    List<ParticleCollisionEvent> events = new();  //파티클 충돌 이벤트는 리스트로 넣어야함

    [SerializeField] private float radius;
    [SerializeField] private float hardness;
    [SerializeField] private float strength;

    private void Awake()
    {
        teamColorInfo = FindObjectOfType<TeamColorInfo>();
        particleSys = GetComponent<ParticleSystem>();
    }

    public void SetTeam(Team team)
    {
        myTeam = team;
    }

    private void OnParticleCollision(GameObject other)
    {
        if (!other.gameObject.TryGetComponent<PaintableObj>(out PaintableObj paintableObj))
        {
            return;
        }

        events.Clear();

        int count = particleSys.GetCollisionEvents(other, events);
        for (int i = 0; i < count; i++)
        {
            Vector3 hitPos = events[i].intersection;
            paintableObj.DrawInk(hitPos, radius, hardness, strength, myTeam);
            //Debug.Log($"페인트 가능 오브젝트 : {paintableObj.name}");
        }

        if (other.gameObject.TryGetComponent<PlayerTestController>(out PlayerTestController player))
        {
            if (player.MyTeam != myTeam)
            {
                Debug.Log("적에게 명중");
            }
            else
            {
                Debug.Log("아군 에게 명중");
            }
        }
    }
}
