using Photon.Pun;
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

    private PlayerController myPlayer; // 내 플레이어 참조

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

        // 내 플레이어를 아직 못 찾았다면 탐색
        if (myPlayer == null)
        {
            FindMyPlayer();
            if (myPlayer == null) return; // 아직도 못찾았으면 Update 종료
        }

        // 플레이어를 찾았다면, 잉크 레벨 업데이트
        if (myPlayer.inkParticleGun != null)
        {
            UpdateInkLevel(myPlayer.inkParticleGun.currentInk, myPlayer.inkParticleGun.maxInk);
        }

        // --- 기존 흔들림 로직 ---
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

    private void FindMyPlayer()
    {
        myPlayer = GetComponentInParent<PlayerController>();
        if (myPlayer != null && myPlayer.photonView.IsMine) return;

        // 부모에 없다면 전체 씬에서 탐색
        var players = FindObjectsOfType<PlayerController>();
        foreach (var player in players)
        {
            if (player.photonView.IsMine)
            {
                myPlayer = player;
                return; // 찾았으면 바로 종료
            }
        }
    }

    // 잉크 탱크의 잔량을 설정
    public void UpdateInkLevel(float currentInk, float maxInk)
    {
        if (inkRenderer == null) return;

        // 셰이더 _FillAmount 값이 0일 때 최대치 1일 때 비어 보이도록 되어있음
        float inkRatio = Mathf.Clamp01(currentInk / maxInk);
        float targetFillAmount = Mathf.Lerp(1f, 0f, inkRatio);
        inkRenderer.material.SetFloat(fill_ID, targetFillAmount);
    }
}