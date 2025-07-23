using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Pun.Demo.Asteroids;
using Photon.Realtime;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerTestController : MonoBehaviourPun, IPunObservable
{
    [SerializeField] private SkinnedMeshRenderer skinRenderer;
    [SerializeField] private Transform cameraPivot;
    [SerializeField] private float moveSpeed;
    [SerializeField] private float mouseSensitivity;
    [SerializeField] private GameObject cameraprefab;
    
    //[SerializeField] private Shooter Shooter;
    [SerializeField] private InkParticleGun inkParticleGun;

    private TeamColorInfo teamColorInfo;
    private Rigidbody rigid;
    private Camera cam;
    private PhotonView weaponView;

    float cameraRotationX = 0;
    float cameraRotationY = 0;

    private Team myTeam = Team.None;
    public Team MyTeam => myTeam;

    private void Awake()
    {
        teamColorInfo = FindObjectOfType<TeamColorInfo>();
    }
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) //�̰� ����?
    {
        if (stream.IsWriting)
        {
            stream.SendNext((int)myTeam);  //�̰� int�� �� �ٲ�?
        }
        else if (stream.IsReading)
        {
            myTeam = (Team)(int)stream.ReceiveNext();
        }
    }

    void Start()
    {
        rigid = GetComponent<Rigidbody>();
        cam = GetComponentInChildren<Camera>();
        weaponView = inkParticleGun.GetComponent<PhotonView>();

        if (photonView.IsMine)
        {
            GameObject mycamera = Instantiate(cameraprefab, cameraPivot);
            mycamera.transform.localPosition = new Vector3(1.2f, 0.8f, -3f);
            Debug.Log("ī�޶� ����");
        }
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        skinRenderer.material.color = teamColorInfo.GetTeamColor(myTeam);

        if (photonView.IsMine)
        {
            Move();
            LookAround();

            if (Input.GetMouseButtonDown(0))
            {
                //photonView.RPC("FireInk", RpcTarget.AllViaServer, myTeam); //����ü�߻�
                weaponView.RPC("FireParticle", RpcTarget.AllViaServer, myTeam, true); //��ƼŬ ����

            }

            if (Input.GetMouseButtonUp(0))
            {
                weaponView.RPC("FireParticle", RpcTarget.AllViaServer, myTeam, false);
            }
        }

        TeamSelect();
    }

    private void TeamSelect( )
    {
        if (photonView.IsMine)
        {
            if (Input.GetKeyDown(KeyCode.Z))
            {
                myTeam = Team.Team1;
                Debug.Log($"������: {myTeam}");
            }
            if (Input.GetKeyDown(KeyCode.X))
            {
                myTeam = Team.Team2;
                Debug.Log($"������: {myTeam}");
            }
        }
        
    }

    private void Move()
    {
        float hotizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 moveDir = (cameraPivot.forward * vertical + cameraPivot.right * hotizontal).normalized;
        moveDir.y = 0;

        rigid.velocity = moveDir * moveSpeed + new Vector3(0, rigid.velocity.y, 0);
    }

    private void LookAround()
    {
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        cameraRotationX += mouseX * mouseSensitivity;
        cameraRotationY -= mouseY * mouseSensitivity;
        cameraRotationY = Mathf.Clamp(cameraRotationY, -30f, 50f);

        cameraPivot.localEulerAngles = new Vector3(cameraRotationY, cameraRotationX, 0);

        Vector3 playerY = transform.eulerAngles;
        playerY.y = cameraPivot.eulerAngles.y;
        transform.eulerAngles = playerY;
    }
}