using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InkProjectile : MonoBehaviour
{
    private Renderer rend;
    [SerializeField] private TeamColorInfo teamColorInfo;

    private Team myTeam;

    private void Awake()
    {
        rend = GetComponent<Renderer>();
        teamColorInfo = FindObjectOfType<TeamColorInfo>();
    }
    public void SetTeam(Team team)
    {
        myTeam = team;

        Color projectileColor = teamColorInfo.GetTeamColor(myTeam);
        rend.material.color = projectileColor;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.TryGetComponent<PaintableObj>(out PaintableObj paintableObj))
        {
            Debug.Log($"페인트 가능 오브젝트 : {paintableObj.name}");
        }

        if (collision.gameObject.TryGetComponent<PlayerTestController>(out PlayerTestController player))
        {
            if(player.MyTeam != myTeam)
            {
                Debug.Log("적에게 명중");
            }
            else
            {
                Debug.Log("아군 에게 명중");
            }
        }

        Destroy(gameObject); //풀로 리턴할 예정
    }

}
