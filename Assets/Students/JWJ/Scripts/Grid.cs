using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid : MonoBehaviour
{
    public Team team = Team.None;

    private void Start()
    {
        Manager.Grid.RegisterGrid(this);
        //그리드를 매니저에 등록
        Debug.Log($"그리드등록 {this.name}");
    }

    public void SetGrid(Team newTeam)
    {
        if(team == newTeam) // 이미 같은팀의 그리드라면
        {
            Debug.Log($"이미 {newTeam}의 영역입니다.");
            return;
        }
        team = newTeam;
        Debug.Log($"{newTeam}으로 그리드 변경.");
        Manager.Grid.UpdateCoverageRate();
        //변경사항 업데이트

    }
}
