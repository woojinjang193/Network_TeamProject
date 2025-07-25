using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DetectModule 
{
    private AIController _controller;
    public Transform Target { get; private set; }
    public bool HasEnemy => Target != null;

    public DetectModule(AIController controller)
    {
        _controller = controller;
    }

    public void Update()
    {
        
        //오버랩스피어로 Player태그를 가진 collider중에서 첫번째 요소를 타겟으로 지정
        Collider[] hits = Physics.OverlapSphere(_controller.transform.position, 10f);
        Target = hits.FirstOrDefault(c => c.CompareTag("Player"))?.transform;

        

        if (Target != null)
        {
            Debug.Log("Player 태그 감지됨");
        }
        //TODO 중립 또는 적 타일을 탐지
        
    }

    //void OnDrawGizmos()
    //{
    //    Gizmos.color = Color.yellow;
    //    Gizmos.DrawWireSphere(_controller.transform.position, 10f);
    //}
    
}
