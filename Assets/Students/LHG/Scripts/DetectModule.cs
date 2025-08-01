using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using UnityEngine;

public class DetectModule
{
    private AIController _controller;
    public Transform Target { get; private set; }
    public Transform TargetGrid { get; private set; }
    public bool HasEnemy => Target != null;
    //public bool HasTargetGrid => TargetGrid != null;

    private float detectRadius = 10f;
    private Team myTeam;


    

    public DetectModule(AIController controller)
    {
        _controller = controller;
    }

    public void Update()
    {
        //DetectEnemy();
        DetectEnemyPlayer();
    }

    private void DetectEnemy()
    {
        //오버랩스피어로 Player태그를 가진 collider중에서 첫번째 요소를 타겟으로 지정=FristOrDefaul
        Collider[] hits = Physics.OverlapSphere(_controller.transform.position, detectRadius);
        Target = hits.FirstOrDefault(c => c.CompareTag("Player"))?.transform;

        if (Target != null)
        {
            Debug.Log("Player 태그 감지됨");
        }
    }

    private void DetectEnemyPlayer()
    {
        Collider[] hits = Physics.OverlapSphere(_controller.transform.position, detectRadius);
        foreach (var hit in hits)
        {
            PlayerController player = hit.GetComponent<PlayerController>();
            if (player != null && player.MyTeam != _controller.MyTeam)
            {
                Target = player.transform;
                Debug.Log($"적 플레이어 감지: {player.photonView.ViewID}");
                return;
            }
        }
        Target = null; //탐지 실패시 target은 null
    }

    private void DetectGrid()
    {


        //기본적으로 발사하고, 우리팀이면 안쏜다 

        //TRYGETVALUE, GETCOMPONENT, COMPARETAG
    }
}
