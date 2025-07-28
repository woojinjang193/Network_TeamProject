using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;

public class DeathState : AIBaseState
{

    public DeathState(AIController controller) : base(controller) { }
    

    public override void OnEnter()
    {
        _controller.GetComponent<Collider>().enabled = false;
        _controller.enabled = false;
    }

    public override void OnUpdate()
    {
        
    }
}
