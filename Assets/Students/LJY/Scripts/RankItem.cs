using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RankItem : MonoBehaviour
{
    public TextMeshProUGUI rankerName;
    public TextMeshProUGUI rankerWinsText;
    public TextMeshProUGUI rankerWinRateText;

    public void Init()
    {
        TextMeshProUGUI[] texts = gameObject.GetComponentsInChildren<TextMeshProUGUI>();
        rankerName =  texts[0];
        rankerWinsText = texts[1];
        rankerWinRateText = texts[2];
    }
    public void SetRankerInfo(string name, int wins, float rate,int rank)
    {
        rankerName.text = rank > 3 ? $"{rank}위 {name}" : name;
        
        rankerWinsText.text = wins.ToString();
        rankerWinRateText.text = $"{rate*100:F}%";
    }
}
