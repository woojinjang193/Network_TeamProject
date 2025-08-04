using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.ExceptionServices;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI;

public class GameResultUI : MonoBehaviour
{
    [SerializeField] private Camera resultCam; //맵을 보여줄 카메라
    [SerializeField] private TMP_Text team1RateText; // 팀1 점유율 텍스트
    [SerializeField] private TMP_Text team2RateText; // 팀2 점유율 텍스트
    [SerializeField] private TMP_Text winnerText; // 승리팀 텍스트

    [SerializeField] private Slider team1Slider;
    [SerializeField] private Slider team2Slider;

    [SerializeField] private float openUIDelayTime = 3f; //유아이 표시 타이밍
    [SerializeField] private float betweenSlideMoveTime = 2f; // 슬라이드 사이 간격
    [SerializeField] private float firstValueShowTime = 2f; //첫번째 점유율 표시 시간 (속도)
    [SerializeField] private float finalValueShowTime = 2f; //마지막 점유율 표시 시간 (속도)

    private string winnerTeam;
    private float firstShowRateValue;
    private float team1RateValue;
    private float team2RateValue;

    private void Start()
    {
        //UI 비활성화
        resultCam.gameObject.SetActive(false);
        team1RateText.gameObject.SetActive(false);
        team2RateText.gameObject.SetActive(false);
        winnerText.gameObject.SetActive(false);
        team1Slider.gameObject.SetActive(false);
        team2Slider.gameObject.SetActive(false);
    }


    public void UIOpen(string winTeam)
    {
        winnerTeam = winTeam;
        resultCam.gameObject.SetActive(true);  //카메라 플레이어에서 분리할 예정??

        //그리드매니저에서 팀 점유율 가져옴
        team1RateValue = Manager.Grid.Team1Rate / 100;
        team2RateValue = Manager.Grid.Team2Rate / 100;
        firstShowRateValue = Mathf.Min(team1RateValue, team2RateValue) * 0.5f;
        //두 수중 작은것의 50%
        StartCoroutine(GameResultUICoroutine());
    }

    IEnumerator GameResultUICoroutine()
    {
        team1Slider.value = 0;
        team2Slider.value = 0;

        yield return new WaitForSeconds(openUIDelayTime);

        //Debug.Log($"첫 밸류: {firstShowRateValue}, 팀1 벨류: {team1RateValue}, 팀2 벨류: {team2RateValue}");

        //UI활성화
        team1RateText.gameObject.SetActive(true);
        team2RateText.gameObject.SetActive(true);
        team1Slider.gameObject.SetActive(true);
        team2Slider.gameObject.SetActive(true);


        float elapsed = 0f;
        while (elapsed < firstValueShowTime)
        {
            elapsed += Time.deltaTime; //타이머
            float time = Mathf.Clamp01(elapsed / firstValueShowTime);
            //최대값 설정
            float value = Mathf.Lerp(0f, firstShowRateValue, time);
            //0에서 firstShowRateValue까지 시간에 따라 증가

            team1Slider.value = value;
            team2Slider.value = value;
            team1RateText.text = (value * 100).ToString("F2") + "%";
            team2RateText.text = (value * 100).ToString("F2") + "%";
            yield return null;
        }

        //수치 보정
        team1Slider.value = firstShowRateValue;
        team2Slider.value = firstShowRateValue;
        team1RateText.text = (firstShowRateValue * 100).ToString("F2") + "%";
        team2RateText.text = (firstShowRateValue * 100).ToString("F2") + "%";

        yield return new WaitForSeconds(betweenSlideMoveTime); //다음 슬라이더 시작까지 딜레이

        float LeftDistance = Mathf.Max(team1RateValue, team2RateValue) - firstShowRateValue;
        //남은 값 (큰값)
        float speed = LeftDistance / finalValueShowTime;
        //속도

        while (team1Slider.value < team1RateValue || team2Slider.value < team2RateValue)
            //더 높은쪽이 끝날때까지
        {
            float delta = speed * Time.deltaTime;
            //프레임동안 슬라이더가 늘어날 양
            
            team1Slider.value = Mathf.Min(team1Slider.value + delta, team1RateValue);
            //최대 team1RateValue 까지 팀1 슬라이더를 delta 만큼 올림
            team2Slider.value = Mathf.Min(team2Slider.value + delta, team2RateValue);
            //최대 team2RateValue 까지 팀2 슬라이더를 delta 만큼 올림

            team1RateText.text = (team1Slider.value * 100f).ToString("F2") + "%";
            team2RateText.text = (team2Slider.value * 100f).ToString("F2") + "%";
            yield return null;
        }

        // 최종 보정
        team1Slider.value = team1RateValue;
        team2Slider.value = team2RateValue;
        team1RateText.text = (team1RateValue * 100f).ToString("F2") + "%";
        team2RateText.text = (team2RateValue * 100f).ToString("F2") + "%";

        winnerText.gameObject.SetActive(true);
        winnerText.text = winnerTeam;
    }

    public void UIClose()
    {
        gameObject.SetActive(false);
    }
}

