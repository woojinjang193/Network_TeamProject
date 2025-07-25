using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AIBaseState
{
    protected AIController _controller;

    public AIBaseState(AIController controller)
    {
        _controller = controller;
    }

    public virtual void OnEnter() { }
    public virtual void OnExit() { }
    public abstract void OnUpdate();
}
