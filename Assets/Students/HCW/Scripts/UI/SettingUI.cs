using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingUI : BaseUI
{
    public override void Open()
    {
        gameObject.SetActive(true);
    }

    public override void Close()
    {
        gameObject.SetActive(false);
    }
}