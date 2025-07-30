using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Die : PlayerState
{
    private const float RESPAWN_TIME = 3.0f; // 리스폰까지 걸리는 시간

    public Player_Die(PlayerController player, StateMachine stateMachine) : base(player, stateMachine) { }

    public override void Enter()
    {
        Debug.Log("사망 상태 진입");

        player.rig.isKinematic = true;
        player.col.enabled = false;

        player.humanModel.SetActive(false);
        player.squidModel.SetActive(false);

        // 로컬 플레이어일 경우에만 카메라 전환 및 리스폰 코루틴 시작
        if (player.photonView.IsMine)
        {
            // 플레이어 카메라 끄고 관전 카메라 켜기
            if (player.playerCameraObject != null) player.playerCameraObject.SetActive(false);
            if (player.spectatorCamera != null) player.spectatorCamera.SetActive(true);

            // 리스폰 타이머
            player.StartCoroutine(RespawnCoroutine());
        }
    }

    private IEnumerator RespawnCoroutine()
    {
        Debug.Log($"{RESPAWN_TIME}초 후 리스폰합니다.");
        yield return new WaitForSeconds(RESPAWN_TIME);

        player.Respawn();
    }

    public override void Exit()
    {
        Debug.Log("사망 상태 종료, 리스폰 준비");

        // 비활성화했던 컴포넌트들 다시 활성화
        player.rig.isKinematic = false;
        player.col.enabled = true;

        // 로컬 플레이어일 경우 카메라 원상복구
        if (player.photonView.IsMine)
        {
            if (player.spectatorCamera != null) player.spectatorCamera.SetActive(false);
            if (player.playerCameraObject != null) player.playerCameraObject.SetActive(true);
        }
    }
}
