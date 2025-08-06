using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using UnityEngine;

public class DetectModule
{
    // 참조
    private AIController _controller;
    public Transform Target { get; set; }
    public Transform TargetGrid { get; private set; }
    
    // 값
    public bool HasEnemy => Target != null;
    
    // 타이머
    public float detectTimer = 0f;

    public DetectModule(AIController controller)
    {
        _controller = controller;
    }

    public void Update()
    {
        if (CheckTimer())
        {
            DetectEnemyPlayer();
        }
    }

    private void DetectEnemyPlayer()
    {
        //해당레이어 컬라이더만 감지
        Collider[] hits = Physics.OverlapSphere(_controller.transform.position, _controller.detectRadius, LayerMask.GetMask("Player"));
        foreach (var hit in hits)
        {
            BaseController player = Manager.Game.GetPlayer(hit);
            if (player != null && player.MyTeam != _controller.MyTeam)
            {
                Target = player.transform;
                Debug.Log($"적 플레이어 감지: {player.photonView.ViewID}");
                return;
            }
        }
        Target = null; //탐지 실패시 target은 null
    }

    private bool CheckTimer()
    {
        if (detectTimer < _controller.detectInterval)
        {
            detectTimer += Time.deltaTime;
            return false;
        }

        detectTimer = 0f;
        return true;
    }

    private void DetectGrid()
    {
        //기본적으로 발사하고, 우리팀이면 안쏜다 

        //TRYGETVALUE, GETCOMPONENT, COMPARETAG
    }
}
