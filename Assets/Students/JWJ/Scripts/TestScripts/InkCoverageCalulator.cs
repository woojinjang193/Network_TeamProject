using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class InkCoverageCalulator : MonoBehaviour
{
    private TeamColorInfo teamColorInfo;
    [SerializeField] private TMP_Text Team1Text;
    [SerializeField] private TMP_Text Team2Text;
    [SerializeField] private float loopIntervalTime = 5f;

    private Color team1Color;
    private Color team2Color;

    private Queue<Texture2D> splatMapQueue = new Queue<Texture2D>();

    private void Awake()
    {
        teamColorInfo = FindObjectOfType<TeamColorInfo>();
        team1Color = teamColorInfo.GetTeamColor(Team.Team1);
        team2Color = teamColorInfo.GetTeamColor(Team.Team2);
    }

    private void Start()
    {
        StartCoroutine(Loop());
    }

    private IEnumerator Loop()
    {
        while (!Manager.Game.IsGameEnd)
        {
            yield return new WaitForSeconds(loopIntervalTime);
            CountTextureColors();
        }
    }

    public void EnqueueSplatMap(Texture2D slapMap)
    {
        splatMapQueue.Enqueue(slapMap);
    }

    public void CountTextureColors()
    {
        int totalPixelNumber = 0;
        int team1Count = 0;
        int team2Count = 0;

        foreach ( var slapMap in FindObjectsOfType<PaintableObj>())
        {
            //Texture2D texture = slapMap
        }
        while (splatMapQueue.Count > 0)
        {
            Texture2D splatMap = splatMapQueue.Dequeue();
            Color[] pixels = splatMap.GetPixels();
            
            totalPixelNumber += pixels.Length;

            foreach (Color color in pixels)
            {
                if (color == team1Color)
                {
                    team1Count++;
                }
                else if (color == team2Color)
                {
                    team2Count++;
                }
            }

            float team1Percentage = (float)team1Count / totalPixelNumber * 100;
            float team2Percentage = (float)team2Count / totalPixelNumber * 100;

            Team1Text.text = Mathf.RoundToInt(team1Percentage) + "%";
            Team2Text.text = Mathf.RoundToInt(team2Percentage) + "%";
        }
    }
}
