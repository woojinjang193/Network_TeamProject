using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InkParticleCollision : MonoBehaviour
{
    private TeamColorInfo teamColorInfo;
    private Team myTeam;
    private void Awake()
    {
        teamColorInfo = FindObjectOfType<TeamColorInfo>();
    }

    public void SetTeam(Team team)
    {
        myTeam = team;
    }

    private void OnParticleCollision(GameObject other)
    {
        if (other.gameObject.TryGetComponent<PaintableObj>(out PaintableObj paintableObj))
        {
            Debug.Log($"����Ʈ ���� ������Ʈ : {paintableObj.name}");
        }

        if (other.gameObject.TryGetComponent<PlayerTestController>(out PlayerTestController player))
        {
            if (player.MyTeam != myTeam)
            {
                Debug.Log("������ ����");
            }
            else
            {
                Debug.Log("�Ʊ� ���� ����");
            }
        }
    }
}
