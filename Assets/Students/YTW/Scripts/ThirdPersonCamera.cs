using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    public Transform cameraTransform;
    public Transform followTransform;

    protected float _offsetX, _offsetY;

    [Header("카메라 설정")]
    [SerializeField]
    protected float followDistance = 6.0f;

    [Tooltip("카메라가 플레이어를 바라볼 높이 오프셋입니다.")]
    [SerializeField]
    protected float heightOffset = 1.5f; // 높이 조절 변수

    [SerializeField]
    protected float yMax = 86f; // 최대 상단 각도
    [SerializeField]
    protected float yMin = -6f; // 최대 하단 각도

    [Header("카메라 감도")]
    [SerializeField]
    protected float xSensitivity = 1.0f;
    [SerializeField]
    protected float ySensitivity = 1.0f;
    [SerializeField]
    protected bool invertY = false;

    protected float _yRotation;
    public float yRotation
    {
        get { return _yRotation; }
    }

    protected void Awake()
    {
        if (followTransform == null)
        {
            return;
        }

        Vector3 targetPosition = followTransform.position + Vector3.up * heightOffset;
        cameraTransform.position = targetPosition + (followTransform.forward * -followDistance);
        cameraTransform.LookAt(targetPosition);
    }

    public void CameraUpdate(float mouseX, float mouseY)
    {
        if (followTransform == null) return;
        _offsetX += mouseX * xSensitivity;
        if (invertY)
        {
            _offsetY += mouseY * ySensitivity; // 마우스를 내리면 위로 (반전 O)
        }
        else
        {
            _offsetY -= mouseY * ySensitivity; // 마우스를 내리면 아래로 (반전 X, 기본값)
        }
        _offsetY = Mathf.Clamp(_offsetY, yMin, yMax);
        Vector3 targetPosition = followTransform.position + Vector3.up * heightOffset;
        Quaternion rotation = Quaternion.Euler(_offsetY, _offsetX, 0);
        Vector3 direction = new Vector3(0, 0, -followDistance);
        cameraTransform.position = targetPosition + (rotation * direction);
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

