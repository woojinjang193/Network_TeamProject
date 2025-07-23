using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Android;

public class InkParticleGun : MonoBehaviourPun
{
    private TeamColorInfo teamColorInfo;
    [SerializeField] private ParticleSystem particleSys;
    [SerializeField] private InkParticleCollision particle;
    private Team myTeam;

    private void Awake()
    {
        teamColorInfo = FindObjectOfType<TeamColorInfo>();
    }

    private void Start()
    {
        ParticleSystem.EmissionModule emission = particleSys.emission;
        emission.enabled = false;
    }

    [PunRPC]
    public void FireParticle(Team team, bool mouseButtonDown)
    {
        //컬러 설정
        myTeam = team;
        Color teamColor = teamColorInfo.GetTeamColor(myTeam);
        ParticleSystemRenderer renderer = particleSys.GetComponent<ParticleSystemRenderer>();
        renderer.material.color = teamColor;

        //파티클 on off 설정
        ParticleSystem.EmissionModule emission = particleSys.emission;
        emission.enabled = mouseButtonDown;

        particle.SetTeam(myTeam);
    }

    
}
