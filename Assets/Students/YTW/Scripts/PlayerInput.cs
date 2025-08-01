using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    public Vector2 MoveInput { get; private set; }
    public Vector2 MouseInput { get; private set; }
    public bool IsJumpPressed { get; private set; }
    public bool IsSquidHeld { get; private set; }
    public bool IsRecenterPressed { get; private set; }
    public bool IsFirePressed { get; private set; }
    public bool IsFireReleased { get; private set; }
    public bool IsFireHeld { get; private set; }

    private PlayerControls playerControls;

    private void Awake()
    {
        playerControls = new PlayerControls();
    }

    private void OnEnable()
    {
        playerControls.Enable();
    }

    private void OnDisable()
    {
        playerControls.Disable();
    }

    void Update()
    {
        MoveInput = playerControls.Player.Movement.ReadValue<Vector2>();
        MouseInput = playerControls.Player.Camera.ReadValue<Vector2>();
        IsJumpPressed = playerControls.Player.Jump.WasPressedThisFrame();
        IsSquidHeld = playerControls.Player.Squid.IsPressed();
        IsRecenterPressed = playerControls.Player.RecenterCamera.WasPressedThisFrame();
        IsFirePressed = playerControls.Player.Fire.WasPressedThisFrame();
        IsFireReleased = playerControls.Player.Fire.WasReleasedThisFrame();
        IsFireHeld = playerControls.Player.Fire.IsPressed();
    }
}
