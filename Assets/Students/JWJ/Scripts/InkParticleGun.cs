using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Android;

public class InkParticleGun : MonoBehaviourPun
{
    private TeamColorInfo teamColorInfo; //팀 컬러 정보
    [SerializeField] private ParticleSystem mainParticle; //잉크 줄기
    [SerializeField] private ParticleSystem fireEffect; // 무기 주변에 잉크가 튀는 연출
    [SerializeField] private InkParticleCollision particleCollision; //잉크 충돌 스크립트
    private Team myTeam;

    private void Awake()
    {
        teamColorInfo = FindObjectOfType<TeamColorInfo>();
        //팀 컬러정보 받아옴
    }

    private void Start()
    {
        ParticleSystem.EmissionModule emission = mainParticle.emission;
        // 게임시작시 파티클을 off 시켜놓기위해 EmissionModule을 받아옴
        ParticleSystem.EmissionModule fireEffectEmission = fireEffect.emission;
        //게임시작시 파티클을 off 시켜놓기위해 EmissionModule을 받아옴
        emission.enabled = false;
        //비활성화
        fireEffectEmission.enabled = false;
        //비활성화

    }

    [PunRPC]
    public void FireParticle(Team team, bool mouseButtonDown) //활성화 & 색 지정
    {
        //파티클 on off 설정
        ParticleSystem.EmissionModule emission = mainParticle.emission;
        //파티클 on off 를 위해 EmissionModule 받아옴
        ParticleSystem.EmissionModule fireEffectEmission = fireEffect.emission;
        //파티클 on off 를 위해 EmissionModule 받아옴
        emission.enabled = mouseButtonDown;
        //비활성화
        fireEffectEmission.enabled = mouseButtonDown;
        //비활성화


        //컬러 설정
        myTeam = team;
        //팀정보 

        Color teamColor = teamColorInfo.GetTeamColor(myTeam);
        //팀정보를 토대로 팀색을 가져옴
        teamColor.a = 1f;
        //블랜딩할때 완전히 덮기위해 알파를 1로 고정


        //mainParticle의 자식 파티클들에게 모두 팀색상을 지정
        ParticleSystem[] particles = mainParticle.GetComponentsInChildren<ParticleSystem>();
        for (int i = 0; i < particles.Length; i++)
        {
            ParticleSystem partSys = particles[i];
            ParticleSystem.MainModule main = partSys.main;
            main.startColor = teamColor;
        }

        particleCollision.SetTeam(myTeam);
        //파티클 충돌 스크립트로 넘겨줌
    }

    
}
