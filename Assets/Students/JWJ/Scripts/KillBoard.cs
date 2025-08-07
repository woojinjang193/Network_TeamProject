using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class KillBoard : MonoBehaviour
{
    [SerializeField] private TMP_Text killText;


    public void KillLog(string text)
    {
        killText.text = text;
    }
    
}
