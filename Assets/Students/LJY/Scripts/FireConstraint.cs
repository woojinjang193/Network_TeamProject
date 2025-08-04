using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Unity.VisualScripting.Dependencies.NCalc;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class FireConstraint : MonoBehaviourPun,IPunObservable
{
    [Header("Set References")]
    [SerializeField] public Rig rigging;
    [SerializeField] public Transform fireTarget;
    
    [Header("Set Values")]
    [SerializeField] private float bodyRotationSpeed = 15f;
    
    
    private PlayerController player;
    private Ray ray;
    private RaycastHit rayHit;
    private Vector3 rayDirection = new(0.5f, 0.5f, 0);
    private Vector3 hitPoint;
    private float rigWeight;
    private float diff;

    // 네트워크 세팅
    private Vector3 networkPos;
    private float deltaPos;
    private float interpolatePos;
    
    void Awake()
    {
        player = GetComponentInParent<PlayerController>();
    }

    void Update()
    {
        if (photonView.IsMine)
        {
            if (player.IsFiring) //공격중이면
            {
                RayCastToCamera(); //카메라 정중앙에 레이캐스트
                ChangeWeight(1f); // weight변경해서 애니메이션 리깅 적용
                RotateModel();
            }
            else
            {
                ChangeWeight(0f); //weight 변경해서 애니메이션 리깅 해제
            }
        }
    }

    void FixedUpdate()
    {
        if (!photonView.IsMine) // 포톤뷰가 본인이 아닐 경우, 타겟 위치 보간 변경
        {
            deltaPos = Vector3.Distance(fireTarget.position, networkPos);
            interpolatePos = deltaPos * Time.deltaTime * PhotonNetwork.SerializationRate;
            
            fireTarget.position = Vector3.MoveTowards(fireTarget.position, networkPos, interpolatePos);

        }
    }
    
    private void RayCastToCamera() // 카메라 정중앙에 레이캐스트하는 함수
    {
        if (Camera.main != null) ray = Camera.main.ViewportPointToRay(rayDirection);
        fireTarget.position = Physics.Raycast(ray, out rayHit, 50f) ? rayHit.point : ray.GetPoint(50f);
    }

    private void ChangeWeight(float weight) // weight 변경함수
    {
        rigWeight = weight;
        diff = Mathf.Abs(rigging.weight - rigWeight);
        if (diff < 0.05)
        {
            rigging.weight = rigWeight;
            return;
        }
        rigging.weight = Mathf.Lerp(rigging.weight, rigWeight, Time.deltaTime*10);
    }

    private void RotateModel()
    {
        Vector3 playerLookDirection = fireTarget.transform.position - player.transform.position;
        playerLookDirection.y = 0;
        Quaternion playerTargetRotation = Quaternion.LookRotation(playerLookDirection);
        player.ModelTransform.rotation = Quaternion.Slerp(player.ModelTransform.rotation, playerTargetRotation, Time.deltaTime * bodyRotationSpeed);

    }
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // 1 에임 타겟 위치 전송
            stream.SendNext(fireTarget.position);
            // 2 현재 Rig weight 전송
            stream.SendNext(rigging.weight);
        }
        else if (stream.IsReading)
        {
            // 1 에임 타겟 위치 수신
            networkPos = (Vector3)stream.ReceiveNext();
            // 2 현재 Rig weight 수신
            rigging.weight = (float)stream.ReceiveNext();
        }
    }
}
