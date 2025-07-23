using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Android;

public class InkParticleGun : MonoBehaviourPun
{
    private TeamColorInfo teamColorInfo;
    [SerializeField] private ParticleSystem mainParticle;
    [SerializeField] private ParticleSystem fireEffect;
    [SerializeField] private InkParticleCollision particle;
    private Team myTeam;

    private void Awake()
    {
        teamColorInfo = FindObjectOfType<TeamColorInfo>();
    }

    private void Start()
    {
        ParticleSystem.EmissionModule emission = mainParticle.emission;
        ParticleSystem.EmissionModule fireEffectEmission = fireEffect.emission;
        emission.enabled = false;
        fireEffectEmission.enabled = false;

    }

    [PunRPC]
    public void FireParticle(Team team, bool mouseButtonDown)
    {
        //파티클 on off 설정
        ParticleSystem.EmissionModule emission = mainParticle.emission;
        ParticleSystem.EmissionModule fireEffectEmission = fireEffect.emission;
        emission.enabled = mouseButtonDown;
        fireEffectEmission.enabled = mouseButtonDown;

        //컬러 설정
        myTeam = team;
        Color teamColor = teamColorInfo.GetTeamColor(myTeam);
        teamColor.a = 1f;

        ParticleSystem[] particles = mainParticle.GetComponentsInChildren<ParticleSystem>();
        for (int i = 0; i < particles.Length; i++)
        {
            ParticleSystem partSys = particles[i];
            ParticleSystem.MainModule main = partSys.main;
            main.startColor = teamColor;
        }

        particle.SetTeam(myTeam);
    }

    
}
