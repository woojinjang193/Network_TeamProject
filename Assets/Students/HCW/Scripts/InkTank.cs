using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InkTank : MonoBehaviour
{
    [Header("Liquid 셰이더가 적용된 자식 오브젝트")]
    public Renderer inkRenderer;

    [Header("흔들림 세팅")]
    public float MaxWobble = 0.03f;
    public float WobbleSpeed = 5.0f;
    public float RecoveryRate = 1f;

    [Header("잉크 잔량")]
    public float MinFillAmount = -0.8f; // 잉크가 완전히 비었을 때
    public float MaxFillAmount = 0.8f; // 잉크가 가득 찼을 때

    private Vector3 prevPos;
    private Vector3 prevRot;
    private float wobbleAmountToAddX;
    private float wobbleAmountToAddZ;

    private int wobbleX_ID;
    private int wobbleZ_ID;
    private int fill_ID;

    void Awake()
    {
        if (inkRenderer == null)
        {
            Debug.LogError("inkRenderer가 비었습니다");
            this.enabled = false;
            return;
        }

        wobbleX_ID = Shader.PropertyToID("_WobbleX");
        wobbleZ_ID = Shader.PropertyToID("_WobbleZ");
        fill_ID = Shader.PropertyToID("_FillAmount");
    }

    private void Update()
    {
        if (inkRenderer == null) return;

        wobbleAmountToAddX = Mathf.Lerp(wobbleAmountToAddX, 0, Time.deltaTime * RecoveryRate);
        wobbleAmountToAddZ = Mathf.Lerp(wobbleAmountToAddZ, 0, Time.deltaTime * RecoveryRate);

        float wobbleAmountX = wobbleAmountToAddX * Mathf.Sin(WobbleSpeed * Time.time);
        float wobbleAmountZ = wobbleAmountToAddZ * Mathf.Sin(WobbleSpeed * Time.time);

        inkRenderer.material.SetFloat(wobbleX_ID, wobbleAmountX);
        inkRenderer.material.SetFloat(wobbleZ_ID, wobbleAmountZ);

        Vector3 moveSpeed = (prevPos - transform.position) / Time.deltaTime;
        Vector3 rotationDelta = transform.rotation.eulerAngles - prevRot;

        wobbleAmountToAddX += Mathf.Clamp((moveSpeed.x + (rotationDelta.z * 0.2f)) * MaxWobble, -MaxWobble, MaxWobble);
        wobbleAmountToAddZ += Mathf.Clamp((moveSpeed.z + (rotationDelta.x * 0.2f)) * MaxWobble, -MaxWobble, MaxWobble);

        prevPos = transform.position;
        prevRot = transform.rotation.eulerAngles;
    }

    // 잉크 탱크의 잔량을 설정
    public void UpdateInkLevel(float currentInk, float maxInk)
    {
        if (inkRenderer == null) return;

        float inkRatio = Mathf.Clamp01(currentInk / maxInk);
        float targetFillAmount = Mathf.Lerp(MinFillAmount, MaxFillAmount, inkRatio);
        inkRenderer.material.SetFloat(fill_ID, targetFillAmount);
    }
}
