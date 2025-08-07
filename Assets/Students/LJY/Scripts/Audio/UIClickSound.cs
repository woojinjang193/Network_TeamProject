using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIClickSound : MonoBehaviour
{
    IEnumerator Start()
    {
        while (Manager.Audio == null)
        {
            yield return null;
        }
        Button btn = gameObject.GetComponent<Button>();
        if (btn)
        {
            btn.onClick.AddListener(Manager.Audio.Click);
        }
    }

}
