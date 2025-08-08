using UnityEngine;
using UnityEngine.InputSystem;

public class ThirdPersonCamera : MonoBehaviour
{
    public Transform cameraTransform;
    public Transform followTransform;
    public PlayerInput playerInput;
    protected float _offsetX, _offsetY;

    [Header("카메라 설정")]
    [SerializeField]
    protected float followDistance = 6.0f;
    [SerializeField]
    protected float heightOffset = 1.5f;
    [SerializeField]
    protected float yMax = 86f;
    [SerializeField]
    protected float yMin = -6f;

    [Header("카메라 감도")]
    [SerializeField]
    protected float mouseXSensitivity = 0.2f;
    [SerializeField]
    protected float mouseYSensitivity = 0.2f;
    [SerializeField]
    protected float gamepadXSensitivity = 2.0f; // 게임패드 X 감도
    [SerializeField]
    protected float gamepadYSensitivity = 2.0f; // 게임패드 Y 감도
    [SerializeField]
    protected bool invertY = false;

    [Header("벽 충돌 설정")]
    [SerializeField]
    private LayerMask collisionLayer;
    [SerializeField]
    private float clipOffset = 0.2f;

    protected float _yRotation;
    public float yRotation => _yRotation;

    protected void Awake()
    {
        if (cameraTransform == null) cameraTransform = transform;
        if (followTransform == null) return;
        if (playerInput == null)
        {
            playerInput = followTransform.GetComponent<PlayerInput>();
        }
        Vector3 targetPosition = followTransform.position + Vector3.up * heightOffset;
        cameraTransform.position = targetPosition + (followTransform.forward * -followDistance);
        cameraTransform.LookAt(targetPosition);
    }

    public void CameraUpdate(float mouseX, float mouseY)
    {
        if (followTransform == null) return;

        float currentXSensitivity = mouseXSensitivity;
        float currentYSensitivity = mouseYSensitivity;

        var lastUsedControl = playerInput.CameraAction.activeControl;

        // 해당 컨트롤이 속한 장치가 게임패드인지 확인
        if (lastUsedControl != null && lastUsedControl.device is Gamepad)
        {
            // 게임패드라면 게임패드 감도를 사용
            currentXSensitivity = gamepadXSensitivity;
            currentYSensitivity = gamepadYSensitivity;
        }
        else
        {
            // 그 외의 경우는 마우스 감도를 사용
            currentXSensitivity = mouseXSensitivity;
            currentYSensitivity = mouseYSensitivity;
        }

        _offsetX += mouseX * currentXSensitivity;
        _offsetY -= (invertY ? -mouseY : mouseY) * currentYSensitivity;
        _offsetY = Mathf.Clamp(_offsetY, yMin, yMax);

        Vector3 targetPosition = followTransform.position + Vector3.up * heightOffset;
        Quaternion rotation = Quaternion.Euler(_offsetY, _offsetX, 0);
        Vector3 direction = new Vector3(0, 0, -followDistance);
        Vector3 desiredPosition = targetPosition + (rotation * direction);


        RaycastHit hit;
        // 플레이어(목표 위치)에서 카메라가 있어야 할 위치로 선(Ray)을 쏩니다.
        if (Physics.Linecast(targetPosition, desiredPosition, out hit, collisionLayer))
        {
            // 벽에 맞았다면, 카메라의 위치를 벽에 맞은 지점에서 약간 앞으로 당겨 설정
            cameraTransform.position = hit.point + hit.normal * clipOffset;
        }
        else
        {
            // 벽에 맞지 않았다면, 원래 계산된 위치로 카메라를 이동
            cameraTransform.position = desiredPosition;
        }


        // 카메라 회전 적용
        cameraTransform.rotation = rotation;
        _yRotation = cameraTransform.rotation.eulerAngles.y;
    }

    public void Recenter(Vector3 targetForward)
    {
        float targetYRotation = Quaternion.LookRotation(targetForward).eulerAngles.y;
        _offsetX = targetYRotation;
        _offsetY = 15.0f;
    }
}
