using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Dependencies.NCalc;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class FireConstraint : MonoBehaviour
{
    [Header("Set References")]
    [SerializeField] private Rig rigging;
    [SerializeField] public Transform fireTarget;
    
    
    private PlayerController player;
    private Animator humanAnimator;
    private Ray ray;
    private RaycastHit rayHit;
    private Vector3 rayDirection = new(0.5f, 0.5f, 0);
    private Vector3 hitPoint;
    private float rigWeight;
    private float diff;

    
    void Awake()
    {
        player = GetComponentInParent<PlayerController>();
        humanAnimator = GetComponent<Animator>();
    }

    void Update()
    {
        if (player.IsFiring) //공격중이면
        {
            RayCastToCamera(); //카메라 정중앙에 레이캐스트
            ChangeWeight(1f); // weight변경해서 애니메이션 리깅 적용
        }
        else
        {
            ChangeWeight(0f); //weight 변경해서 애니메이션 리깅 해제
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

}
