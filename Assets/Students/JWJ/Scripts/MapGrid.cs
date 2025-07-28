using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public class MapGrid : MonoBehaviour
{
    public Team team = Team.None;

    private void Start()
    {
        Manager.Grid.RegisterGrid(this);
        //그리드를 매니저에 등록
        //Debug.Log($"그리드등록 {this.name}");
    }

    public void SetGrid(Team newTeam)
    {
        if(team == newTeam) // 이미 같은팀의 그리드라면
        {
            //Debug.Log($"이미 {newTeam}의 영역입니다.");
            return;
        }
        Team oldTeam = team;
        //팀 정보 변경 전 원래 팀을 oldTeam에 저장
        team = newTeam;
        //새 팀을 업데이트
        //Debug.Log($"{newTeam}으로 그리드 변경.");
        Manager.Grid.ChangeGridTeam(oldTeam, newTeam);
        //변경사항 업데이트
    }
}
