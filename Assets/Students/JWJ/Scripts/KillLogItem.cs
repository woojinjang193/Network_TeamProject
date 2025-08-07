using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class KillLogItem : MonoBehaviour
{
    [SerializeField] private TMP_Text killerNameText;
    [SerializeField] private TMP_Text victimNameText;
    [SerializeField] private Image deathCauseImage;

    public void KillLogItemSet(string killer, string victim, Sprite causeSprite)
    {
        killerNameText.text = killer;
        victimNameText.text = victim;
        deathCauseImage.sprite = causeSprite;
    }
}
