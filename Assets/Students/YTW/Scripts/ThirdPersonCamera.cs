using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    public Transform cameraTransform;
    public Transform followTransform;

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
    protected float xSensitivity = 1.0f;
    [SerializeField]
    protected float ySensitivity = 1.0f;
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

        Vector3 targetPosition = followTransform.position + Vector3.up * heightOffset;
        cameraTransform.position = targetPosition + (followTransform.forward * -followDistance);
        cameraTransform.LookAt(targetPosition);
    }

    public void CameraUpdate(float mouseX, float mouseY)
    {
        if (followTransform == null) return;

        _offsetX += mouseX * xSensitivity;
        _offsetY -= (invertY ? -mouseY : mouseY) * ySensitivity;
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
