public abstract class BaseState
{
    public bool HasPhysics { get; protected set; } = false;

    public abstract void Enter();
    public abstract void Update();
    public abstract void FixedUpdate();
    public abstract void Exit();
}

