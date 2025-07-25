using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIStateMachine : MonoBehaviour 
{
    private AIBaseState _currentState;

    public void SetState(AIBaseState newState)
    {
        _currentState?.OnExit();
        _currentState = newState;
        _currentState?.OnEnter();
    }

    public void Update()
    {
        _currentState?.OnUpdate();
    }
}
