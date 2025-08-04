using UnityEngine;

public class AimController : MonoBehaviour
{
    [Header("컴포넌트 연결")]
    public PlayerController player;

    [Header("회전 설정")]
    public float lookAtWeight = 0.8f;
    public float bodyRotationSpeed = 15f;

    private Animator humanAnimator;
    private Camera mainCamera;
    private Transform weaponTransform;
    private Transform muzzleTransform;
    private Vector3 targetPoint; 

    void Awake()
    {
        if (player == null) player = GetComponentInParent<PlayerController>();
        humanAnimator = GetComponent<Animator>();
    }

    void Start()
    {
        if (player != null)
        {
            mainCamera = player.mainCamera;
            weaponTransform = player.weaponTransform;
            muzzleTransform = player.muzzleTransform;
        }
    }

    // 모든 회전 로직을 여기서 순서대로 처리하여 충돌을 방지
    void LateUpdate()
    {
        if (player == null || mainCamera == null) return;

        // 조준점을 계산하여 targetPoint 변수에 저장
        Ray ray = mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;
        LayerMask playerLayer = LayerMask.NameToLayer("Player");
        int layerMask = ~(1 << playerLayer);

        if (Physics.Raycast(ray, out hit, 200f, layerMask))
        {
            targetPoint = hit.point;
        }
        else
        {
            targetPoint = ray.GetPoint(200f);
        }

        if (player.IsFiring)
        {

            Vector3 playerLookDirection = mainCamera.transform.forward;
            playerLookDirection.y = 0;
            Quaternion playerTargetRotation = Quaternion.LookRotation(playerLookDirection);
            player.modelRoot.rotation = Quaternion.Slerp(player.modelRoot.rotation, playerTargetRotation, Time.deltaTime * bodyRotationSpeed);

            // 총을 정확히 목표 지점을 향하도록 최종적으로 회전
            if (weaponTransform != null)
            {
                Vector3 directionToTarget;
                if (muzzleTransform != null)
                {
                    directionToTarget = targetPoint - muzzleTransform.position;
                }
                else
                {
                    directionToTarget = targetPoint - weaponTransform.position;
                }

                Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
                weaponTransform.rotation = targetRotation;
            }
        }
    }

    // OnAnimatorIK는 이제 오직 상체를 자연스럽게 보이도록 하는 시각적 역할만 담당
    void OnAnimatorIK(int layerIndex)
    {
        if (player == null || !player.IsFiring || humanAnimator == null)
        {
            if (humanAnimator != null)
            {
                humanAnimator.SetLookAtWeight(0);
            }
            return;
        }
        humanAnimator.SetLookAtWeight(lookAtWeight);
        humanAnimator.SetLookAtPosition(targetPoint);
    }
}
