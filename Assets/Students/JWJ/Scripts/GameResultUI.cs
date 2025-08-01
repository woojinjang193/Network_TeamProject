using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.ExceptionServices;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameResultUI : MonoBehaviour
{
    [SerializeField] private Camera resultCam;
    [SerializeField] private TMP_Text team1RateText;
    [SerializeField] private TMP_Text team2RateText;
    [SerializeField] private TMP_Text winnerText;

    [SerializeField] private Slider team1Slider;
    [SerializeField] private Slider team2Slider;

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
        Debug.Log("게임결과 UI");
        winnerTeam = winTeam;
        resultCam.gameObject.SetActive(true);  //카메라 플레이어에서 분리할 예정??

        //그리드매니저에서 팀 점유율 가져옴
        team1RateValue = Manager.Grid.Team1Rate / 100;
        team2RateValue = Manager.Grid.Team2Rate / 100;
        StartCoroutine(GameResultUICoroutine());
    }

    IEnumerator GameResultUICoroutine()
    {
        team1Slider.value = 0;
        team2Slider.value = 0;

        firstShowRateValue = Mathf.Min(team1RateValue, team2RateValue) * 0.5f;
        //두 수중 작은것의 50%

        yield return new WaitForSeconds(3);
        Debug.Log($"첫 밸류: {firstShowRateValue}, 팀1 벨류: {team1RateValue}, 팀2 벨류: {team2RateValue}");

        //UI활성화
        team1RateText.gameObject.SetActive(true);
        team2RateText.gameObject.SetActive(true);
        team1Slider.gameObject.SetActive(true);
        team2Slider.gameObject.SetActive(true);

        yield return new WaitForSeconds(2);

        while (team1Slider.value < firstShowRateValue && team2Slider.value < firstShowRateValue)
        {
            //Debug.Log("while도는중");
            team1Slider.value += 0.0025f;
            team2Slider.value += 0.0025f;

            team1RateText.text = (team1Slider.value * 100f).ToString("F2") + "%";
            team2RateText.text = (team2Slider.value * 100f).ToString("F2") + "%";

            yield return null;
        }

        yield return new WaitForSeconds(2);

        while (team1Slider.value < team1RateValue - 0.0030f || team2Slider.value < team2RateValue - 0.0030f)
        {
            if (team1Slider.value < team1RateValue)
            {
                team1Slider.value += 0.0025f;
            }

            if (team2Slider.value < team2RateValue)
            {
                team2Slider.value += 0.0025f;
            }

            team1RateText.text = (team1Slider.value * 100f).ToString("F2") + "%";
            team2RateText.text = (team2Slider.value * 100f).ToString("F2") + "%";

            yield return null;
        }

        team1RateText.text = (team1RateValue * 100).ToString("F2") + "%";
        team2RateText.text = (team2RateValue * 100).ToString("F2") + "%";
        winnerText.gameObject.SetActive(true);
        winnerText.text = winnerTeam;

    }

    public void UIClose()
    {
        gameObject.SetActive(false);
    }
}
