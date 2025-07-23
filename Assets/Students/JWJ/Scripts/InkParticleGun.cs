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
        //�÷� ����
        myTeam = team;
        Color teamColor = teamColorInfo.GetTeamColor(myTeam);
        ParticleSystemRenderer renderer = particleSys.GetComponent<ParticleSystemRenderer>();
        renderer.material.color = teamColor;

        //��ƼŬ on off ����
        ParticleSystem.EmissionModule emission = particleSys.emission;
        emission.enabled = mouseButtonDown;

        particle.SetTeam(myTeam);
    }

    
}
