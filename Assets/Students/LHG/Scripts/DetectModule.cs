using System.Collections;
using System.Collections.Generic;
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
    public bool HasTargetGrid => TargetGrid != null;
    
    // 타이머
    private float detectTimer = 0f;
    

    public DetectModule(AIController controller)
    {
        _controller = controller;
    }

    public void Update()
    {
        if (_controller.detectInterval > detectTimer)
        {
            detectTimer += Time.deltaTime;
        }
        else
        {
            DetectEnemy();
            detectTimer = 0f;
        }
        
    }

    private void DetectEnemy()
    {
        //오버랩스피어로 Player태그를 가진 collider중에서 첫번째 요소를 타겟으로 지정=FristOrDefaul
        Collider[] hits = Physics.OverlapSphere(_controller.transform.position, _controller.detectRadius);
        Target = hits.FirstOrDefault(c => c.CompareTag("Player"))?.transform;

        if (Target != null)
        {
            Debug.Log("Player 태그 감지됨");
        }
        else
        {
            Debug.Log($"플레이어 감지 못함{HasEnemy}");
        }
    }

    private void DetectGrid()
    {


        //기본적으로 발사하고, 우리팀이면 안쏜다 

        //TRYGETVALUE, GETCOMPONENT, COMPARETAG
    }
}
