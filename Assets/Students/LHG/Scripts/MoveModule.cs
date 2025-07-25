using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveModule
{
    private AIController _controller;
    public MoveModule(AIController controller)
    {
        _controller = controller;
    }

    public void Wander()
    {
        //Wander가 필요한가?
        //필요하다면 탐지시 중립타일도, 적타일도, 적도 없을 때 필요한데..
        //탐지범위를 확장하는게 낫지 않을까?
        //랜덤방향이동
    }

    public void MoveTo(Vector3 targetPos)
    {
        //타겟위치로 이동
    }
}
