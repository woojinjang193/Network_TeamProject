using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using Cinemachine;

public class Player_Die : PlayerState
{
    private CinemachineVirtualCamera _vcamSpectator;
    private CinemachineBrain _cinemachineBrain;
    private Camera _myPlayerCamera;

    // '가짜 출발지 VCam'을 담을 임시 게임 오브젝트
    private GameObject _dummyFromCameraObject;

    public Player_Die(PlayerController player, StateMachine stateMachine) : base(player, stateMachine) { }

    public override void Enter()
    {
        Debug.Log("사망 상태 진입");

        player.rig.isKinematic = true;
        player.col.enabled = false;
        player.humanModel.SetActive(false);
        player.squidModel.SetActive(false);

        if (player.photonView.IsMine)
        {
            player.StartCoroutine(SwitchToSpectatorCamera());
            player.StartCoroutine(RespawnCoroutine());
        }
    }

    private IEnumerator SwitchToSpectatorCamera()
    {
        // 1. 플레이어 카메라 찾기
        if (player.playerCameraObject != null)
        {
            _myPlayerCamera = player.playerCameraObject.GetComponentInChildren<Camera>();
        }
        else
        {
            Debug.LogError("Player Camera Object를 찾을 수 없습니다.");
            yield break;
        }

        // 2. 현재 카메라 위치에 '가짜 출발지 VCam'을 생성
        _dummyFromCameraObject = new GameObject("Temp_From_VCam");
        _dummyFromCameraObject.transform.position = _myPlayerCamera.transform.position;
        _dummyFromCameraObject.transform.rotation = _myPlayerCamera.transform.rotation;
        var vcamFrom = _dummyFromCameraObject.AddComponent<CinemachineVirtualCamera>();
        vcamFrom.Priority = 11; // 현재 가장 높은 우선순위로 설정

        // 3. '내 카메라'에 CinemachineBrain을 추가하고 블렌딩을 설정
        _cinemachineBrain = _myPlayerCamera.gameObject.AddComponent<CinemachineBrain>();
        _cinemachineBrain.m_DefaultBlend.m_Style = CinemachineBlendDefinition.Style.EaseInOut;
        _cinemachineBrain.m_DefaultBlend.m_Time = 1.5f; // n초 동안 부드럽게 전환

        // 4. 기존 카메라 스크립트의 제어권을 비활성화
        if (player.tpsCamera != null)
        {
            player.tpsCamera.enabled = false;
        }

        // 5. Brain이 '가짜 출발지 VCam'을 인식할 때까지 한 프레임 대기
        yield return null;

        // 6. 진짜 '목적지 VCam'을 찾아서 더 높은 우선순위로 활성화
        CinemachineVirtualCamera[] vcams = Resources.FindObjectsOfTypeAll<CinemachineVirtualCamera>();
        foreach (var vcam in vcams)
        {
            if (vcam.gameObject.name == "VCam_Spectator")
            {
                _vcamSpectator = vcam;
                _vcamSpectator.gameObject.SetActive(true);
                _vcamSpectator.Priority = 12; // '가짜 출발지'보다 높게 설정하여 전환을 유발
                break;
            }
        }
    }

    public override void Exit()
    {
        player.IsDead = false;
        Debug.Log("사망 상태 종료, 리스폰 준비");

        player.rig.isKinematic = false;
        player.col.enabled = true;
        player.humanModel.SetActive(true);

        if (player.photonView.IsMine)
        {
            // 모든 임시 컴포넌트와 오브젝트를 깨끗하게 정리
            if (_vcamSpectator != null)
            {
                _vcamSpectator.gameObject.SetActive(false);
            }
            if (_cinemachineBrain != null)
            {
                Object.Destroy(_cinemachineBrain);
            }
            if (_dummyFromCameraObject != null)
            {
                Object.Destroy(_dummyFromCameraObject);
            }
            if (player.tpsCamera != null)
            {
                player.tpsCamera.enabled = true;
            }
        }
    }

    private IEnumerator RespawnCoroutine()
    {
        Debug.Log($"{PlayerController.RESPAWN_TIME}초 후 리스폰합니다.");
        yield return new WaitForSeconds(PlayerController.RESPAWN_TIME);
        Debug.Log($"리스폰 시간{PlayerController.RESPAWN_TIME}");

        player.IsDeadState = false;
        player.Respawn();
    }
}
